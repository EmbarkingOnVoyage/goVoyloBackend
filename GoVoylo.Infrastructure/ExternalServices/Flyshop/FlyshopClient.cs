using System.Globalization;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using Microsoft.Extensions.Options;

namespace GoVoylo.Infrastructure.ExternalServices.Flyshop
{
    public class FlyshopClient : IFlightSupplierClient
    {
        // 0 = domestic (all segments India-India), 1 = international — the
        // conventional meaning of Travel_Type for suppliers like this one.
        // NOTE: confirmed only that domestic (TravelType=0) searches like DEL-BOM
        // work. BOM-DXB still returns "9999: travel type seems invalid" with every
        // value tried (0, 1, 2) — so either this staging account isn't entitled to
        // international content, or the real failing field is something else
        // entirely and Flyshop's error text is misleading. Needs their confirmation
        // before trusting this domestic/international split for international routes.
        private static readonly HashSet<string> IndianAirportCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            "DEL", "BOM", "NMI", "BLR", "HYD", "MAA", "CCU", "PNQ", "AMD", "JAI",
            "GOI", "COK", "LKO", "IXC", "GAU", "PAT", "IXE", "BHO", "DXN", "HDO",
            "NAG", "IDR", "VNS", "ATQ", "TRV", "VTZ", "IXR", "RPR", "BBI", "SXR",
            "IXB", "IXJ", "STV", "UDR", "JDH", "JLR", "IXA", "IXZ", "IXM", "IXU",
        };

        private static bool IsDomesticItinerary(IReadOnlyList<FlightSearchSegmentDto> segments) =>
            segments.All(s => IndianAirportCodes.Contains(s.Origin) && IndianAirportCodes.Contains(s.Destination));

        private readonly HttpClient _httpClient;
        private readonly FlyshopOptions _options;

        public FlyshopClient(HttpClient httpClient, IOptions<FlyshopOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public string SupplierCode => FlightSupplierCodes.Flyshop;

        public async Task<SupplierFlightSearchResultDto> SearchAsync(
            FlightSearchRequestDto request, CancellationToken cancellationToken)
        {
            var wireRequest = new AirSearchRequestWire
            {
                AuthHeader = BuildAuthHeader(),
                TravelType = IsDomesticItinerary(request.Segments) ? 0 : 1,
                BookingType = MapBookingType(request.TripType),
                TripInfo = request.Segments
                    .Select((s, index) => new TripInfoWire
                    {
                        Origin = s.Origin,
                        Destination = s.Destination,
                        TravelDate = s.TravelDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                        TripId = index
                    })
                    .ToList(),
                AdultCount = request.AdultCount.ToString(CultureInfo.InvariantCulture),
                ChildCount = request.ChildCount.ToString(CultureInfo.InvariantCulture),
                InfantCount = request.InfantCount.ToString(CultureInfo.InvariantCulture),
                ClassOfTravel = MapClassOfTravel(request.CabinClass),
                InventoryType = 0,
                SourceType = 0,
                FilteredAirline = new List<FilteredAirlineWire> { new() { AirlineCode = string.Empty } }
            };

            var wireResponse = await PostAsync<AirSearchRequestWire, AirSearchResponseWire>(
                "Air_Search", wireRequest, cancellationToken);

            // "0003" is Flyshop's documented code for "no flights for this search" —
            // a normal empty result, not a supplier/integration failure. Every other
            // non-"0000" code is still treated as an unexpected error by EnsureSuccess.
            if (wireResponse.ResponseHeader?.ErrorCode == "0003")
            {
                return new SupplierFlightSearchResultDto(wireResponse.SearchKey, new List<SupplierFlightOptionDto>());
            }

            EnsureSuccess(wireResponse.ResponseHeader, "Air_Search");

            var flights = wireResponse.TripDetails
                .SelectMany(t => t.Flights.Select(f => MapFlight(f, t.TripId ?? 0, request)))
                .ToList();

            return new SupplierFlightSearchResultDto(wireResponse.SearchKey, flights);
        }

        public async Task<SupplierRepriceResultDto> RepriceAsync(
            SupplierRepriceRequestDto request, CancellationToken cancellationToken)
        {
            var wireRequest = new AirRepriceRequestWire
            {
                AuthHeader = BuildAuthHeader(),
                SearchKey = request.SearchKey,
                AirRepriceRequests = new List<AirRepriceRequestItemWire>
                {
                    new() { FlightKey = request.FlightKey, FareId = request.FareId }
                },
                CustomerMobile = _options.CustomerMobile,
                GstInput = false,
                SinglePricing = true
            };

            var wireResponse = await PostAsync<AirRepriceRequestWire, AirRepriceResponseWire>(
                "Air_Reprice", wireRequest, cancellationToken);

            EnsureSuccess(wireResponse.ResponseHeader, "Air_Reprice");

            var repriced = wireResponse.AirRepriceResponses.FirstOrDefault()?.Flight;

            if (repriced == null)
            {
                throw new InvalidOperationException("Flyshop Air_Reprice returned no repriced flight.");
            }

            var mapped = MapFlight(repriced);

            return new SupplierRepriceResultDto(
                mapped.FlightKey,
                mapped.FareId,
                mapped.TotalAmount,
                mapped.CurrencyCode,
                repriced.Repriced,
                repriced.IsFareChange);
        }

        // No batching of Flyshop's own — each leg is independent, so this just runs
        // the existing single-leg RepriceAsync for every request in order. Identical
        // behavior/result to the caller looping over RepriceAsync itself.
        public async Task<IReadOnlyList<SupplierRepriceResultDto>> RepriceBatchAsync(
            IReadOnlyList<SupplierRepriceRequestDto> requests, CancellationToken cancellationToken)
        {
            var results = new List<SupplierRepriceResultDto>(requests.Count);

            foreach (var request in requests)
            {
                results.Add(await RepriceAsync(request, cancellationToken));
            }

            return results;
        }

        public async Task<SupplierLowFareResultDto> GetLowFareCalendarAsync(
            SupplierLowFareRequestDto request, CancellationToken cancellationToken)
        {
            var wireRequest = new AirLowFareRequestWire
            {
                AuthHeader = BuildAuthHeader(),
                Origin = request.Origin,
                Destination = request.Destination,
                Month = request.Month.ToString("D2", CultureInfo.InvariantCulture),
                Year = request.Year
            };

            var wireResponse = await PostAsync<AirLowFareRequestWire, AirLowFareResponseWire>(
                "Air_LowFare", wireRequest, cancellationToken);

            EnsureSuccess(wireResponse.ResponseHeader, "Air_LowFare");

            var days = (wireResponse.LowFares ?? new List<LowFareWire>())
                .Select(f => new SupplierLowFareDayDto(
                    ParseDate(f.TravelDate),
                    f.Amount,
                    "INR",
                    f.AirlineCode ?? string.Empty,
                    f.AirlinesName ?? string.Empty))
                .Where(d => d.TravelDate != default)
                .ToList();

            return new SupplierLowFareResultDto(days);
        }

        public async Task<SupplierAncillaryResultDto> GetAncillariesAsync(
            SupplierAncillaryRequestDto request, CancellationToken cancellationToken)
        {
            var wireRequest = new AirSsrRequestWire
            {
                AuthHeader = BuildAuthHeader(),
                SearchKey = request.SearchKey,
                AirSsrRequestDetails = new List<AirSsrRequestItemWire>
                {
                    new() { FlightKey = request.FlightKey }
                }
            };

            var wireResponse = await PostAsync<AirSsrRequestWire, AirSsrResponseWire>(
                "Air_GetSSR", wireRequest, cancellationToken);

            EnsureSuccess(wireResponse.ResponseHeader, "Air_GetSSR");

            var options = wireResponse.SsrFlightDetails
                .SelectMany(f => f.SsrDetails)
                .Select(MapSsrDetail)
                .ToList();

            return new SupplierAncillaryResultDto(options);
        }

        public async Task<SupplierSeatMapResultDto> GetSeatMapAsync(
            SupplierSeatMapRequestDto request, CancellationToken cancellationToken)
        {
            var wireRequest = new AirSeatMapRequestWire
            {
                AuthHeader = BuildAuthHeader(),
                SearchKey = request.SearchKey,
                FlightKeys = new List<string> { request.FlightKey },
                PaxDetails = request.Travelers
                    .Select(t => new PaxDetailWire
                    {
                        PaxId = t.PaxId,
                        PaxType = t.PaxType,
                        Title = t.Title,
                        FirstName = t.FirstName,
                        LastName = t.LastName,
                        Gender = t.Gender
                    })
                    .ToList()
            };

            var wireResponse = await PostAsync<AirSeatMapRequestWire, AirSeatMapResponseWire>(
                "Air_GetSeatMap", wireRequest, cancellationToken);

            EnsureSuccess(wireResponse.ResponseHeader, "Air_GetSeatMap");

            var segments = wireResponse.AirSeatMaps
                .SelectMany(m => m.SeatSegments)
                .Select(seg => new SupplierSeatSegmentDto(
                    seg.LegIndex,
                    seg.SeatRow
                        .Select(row => new SupplierSeatRowDto(row.SeatDetails.Select(MapSsrDetail).ToList()))
                        .ToList()))
                .ToList();

            return new SupplierSeatMapResultDto(segments);
        }

        public async Task<SupplierTempBookingResultDto> CreateTempBookingAsync(
            SupplierTempBookingRequestDto request, CancellationToken cancellationToken)
        {
            var wireRequest = new AirTempBookingRequestWire
            {
                AuthHeader = BuildAuthHeader(),
                CustomerMobile = _options.CustomerMobile,
                PassengerMobile = request.PassengerMobile,
                PassengerEmail = request.PassengerEmail,
                PaxDetails = request.Travelers
                    .Select(t => new TempBookingPaxDetailWire
                    {
                        PaxId = t.PaxId,
                        PaxType = t.PaxType,
                        Title = t.Title,
                        FirstName = t.FirstName,
                        LastName = t.LastName,
                        Gender = t.Gender,
                        Dob = t.DateOfBirth?.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                        PassportNumber = t.PassportNumber,
                        // Nationality doubles as issuing country — see
                        // BookingTravelerRequestDto's own doc comment for why.
                        PassportIssuingCountry = t.PassportNationality,
                        PassportExpiry = t.PassportExpiry?.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                        Nationality = t.PassportNationality,
                        PancardNumber = t.PanNumber
                    })
                    .ToList(),
                Gst = request.Gst,
                GstNumber = request.GstNumber,
                GstHolderName = request.GstHolderName,
                GstAddress = request.GstAddress,
                BookingFlightDetails = request.Flights
                    .Select(f => new BookingFlightDetailWire
                    {
                        SearchKey = f.SearchKey,
                        FlightKey = f.FlightKey,
                        BookingSsrDetails = f.SelectedSsrs
                            .Select(s => new BookingSsrDetailWire { PaxId = s.PaxId, SsrKey = s.SsrKey })
                            .ToList()
                    })
                    .ToList()
            };

            var wireResponse = await PostAsync<AirTempBookingRequestWire, AirTempBookingResponseWire>(
                "Air_TempBooking", wireRequest, cancellationToken);

            EnsureSuccess(wireResponse.ResponseHeader, "Air_TempBooking");

            if (string.IsNullOrEmpty(wireResponse.BookingRefNo))
            {
                throw new InvalidOperationException("Flyshop Air_TempBooking returned no booking reference.");
            }

            return new SupplierTempBookingResultDto(wireResponse.BookingRefNo);
        }

        // Flyshop's certification requires its direct booking flow (Air_TempBooking →
        // AddPayment → Air_Ticketing "1"), not a Block_Ticket ("0") hold first — so
        // nothing is placed here. The temp booking is reported as Status 33 (awaiting
        // payment, as VerifyRazorpayPaymentCommandHandler expects) with no legs:
        // Flight_Ids only exist once Air_Ticketing runs after payment.
        public Task<SupplierTicketingResultDto> CreateBlockTicketAsync(
            string bookingRefNo, CancellationToken cancellationToken) =>
            Task.FromResult(new SupplierTicketingResultDto(
                bookingRefNo, "33", null, null, null, null, null, Array.Empty<SupplierTicketingLegResultDto>()));

        // Flyshop's temp booking is ticketed directly, so deferredBookPayload is
        // always null here.
        public Task<SupplierTicketingResultDto> BookTicketAsync(
            string bookingRefNo, string? deferredBookPayload, CancellationToken cancellationToken) =>
            TicketAsync(bookingRefNo, ticketingType: "1", cancellationToken);

        private async Task<SupplierTicketingResultDto> TicketAsync(
            string bookingRefNo, string ticketingType, CancellationToken cancellationToken)
        {
            var wireRequest = new AirTicketingRequestWire
            {
                AuthHeader = BuildAuthHeader(),
                BookingRefNo = bookingRefNo,
                TicketingType = ticketingType
            };

            var wireResponse = await PostAsync<AirTicketingRequestWire, AirTicketingResponseWire>(
                "Air_Ticketing", wireRequest, cancellationToken);

            EnsureSuccess(wireResponse.ResponseHeader, "Air_Ticketing");

            var legs = wireResponse.AirlinePnrDetails
                .Select(d =>
                {
                    var legPnr = d.AirlinePnrs.FirstOrDefault();
                    return new SupplierTicketingLegResultDto(
                        d.FlightId ?? string.Empty,
                        d.StatusId ?? string.Empty,
                        legPnr?.AirlineCode,
                        legPnr?.AirlinePnr,
                        legPnr?.CrsPnr,
                        legPnr?.RecordLocator,
                        d.FailureRemark);
                })
                .ToList();

            var detail = wireResponse.AirlinePnrDetails.FirstOrDefault();
            var pnr = detail?.AirlinePnrs.FirstOrDefault();

            return new SupplierTicketingResultDto(
                wireResponse.BookingRefNo ?? bookingRefNo,
                detail?.StatusId ?? string.Empty,
                pnr?.AirlineCode,
                pnr?.AirlinePnr,
                pnr?.CrsPnr,
                pnr?.RecordLocator,
                detail?.FailureRemark,
                legs);
        }

        // AddPayment lives on Flyshop's separate tradehost/TradeAPIService.svc, not
        // the airlinehost/AirAPIService.svc every other call here uses — passing the
        // full absolute URL through PostAsync bypasses _httpClient.BaseAddress for
        // just this one call (HttpClient treats an absolute request URI as-is).
        public async Task<SupplierPaymentResultDto> AddPaymentAsync(
            string bookingRefNo, string clientRefNo, string? deferredBookPayload, CancellationToken cancellationToken)
        {
            var wireRequest = new AddPaymentRequestWire
            {
                AuthHeader = BuildAuthHeader(),
                RefNo = bookingRefNo,
                ClientRefNo = clientRefNo
            };

            var wireResponse = await PostAsync<AddPaymentRequestWire, AddPaymentResponseWire>(
                $"{_options.TradeBaseUrl}AddPayment", wireRequest, cancellationToken);

            EnsureSuccess(wireResponse.ResponseHeader, "AddPayment");

            return new SupplierPaymentResultDto(
                wireResponse.Amount,
                wireResponse.PaymentId,
                wireResponse.ResponseHeader?.StatusId ?? string.Empty);
        }

        public async Task<SupplierFareRuleResultDto> GetFareRulesAsync(
            SupplierFareRuleRequestDto request, CancellationToken cancellationToken)
        {
            var wireRequest = new AirFareRuleRequestWire
            {
                AuthHeader = BuildAuthHeader(),
                SearchKey = request.SearchKey,
                FlightKey = request.FlightKey,
                FareId = request.FareId
            };

            var wireResponse = await PostAsync<AirFareRuleRequestWire, AirFareRuleResponseWire>(
                "Air_FareRule", wireRequest, cancellationToken);

            EnsureSuccess(wireResponse.ResponseHeader, "Air_FareRule");

            var rules = wireResponse.FareRules
                .Select(r => new SupplierFareRuleDto(
                    r.SegmentId ?? string.Empty,
                    r.FareRuleName ?? string.Empty,
                    StripHtml(r.FareRuleDesc)))
                .ToList();

            return new SupplierFareRuleResultDto(rules);
        }

        // FareRuleDesc arrives as a full XHTML document (doctype, head, inline
        // <style>, the lot) wrapping what's usually one short plain-text paragraph —
        // strip markup down to readable text rather than pull in an HTML renderer
        // for what the supplier's own sample shows is trivial boilerplate content.
        private static string StripHtml(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var withoutStyle = Regex.Replace(html, "<style[^>]*>.*?</style>", " ", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var withoutTags = Regex.Replace(withoutStyle, "<[^>]+>", " ");
            var decoded = System.Net.WebUtility.HtmlDecode(withoutTags);
            return Regex.Replace(decoded, @"\s+", " ").Trim();
        }

        public async Task<SupplierCancellationResultDto> CancelBookingAsync(
            SupplierCancellationRequestDto request, CancellationToken cancellationToken)
        {
            var wireRequest = new AirTicketCancellationRequestWire
            {
                AuthHeader = BuildAuthHeader(),
                AirTicketCancelDetails = request.Segments
                    .Select(s => new AirTicketCancelDetailWire
                    {
                        FlightId = s.FlightId,
                        PassengerId = s.PassengerId,
                        SegmentId = s.SegmentId
                    })
                    .ToList(),
                AirlinePnr = request.AirlinePnr,
                RefNo = request.RefNo,
                CancelCode = request.CancelCode,
                ReqRemarks = request.ReqRemarks,
                CancellationType = request.CancellationType
            };

            // Endpoint name really is Air_TicketCancellation, not Air_Cancellation —
            // confirmed from the collection's own sample URL, not just its sidebar label.
            var wireResponse = await PostAsync<AirTicketCancellationRequestWire, AirTicketCancellationResponseWire>(
                "Air_TicketCancellation", wireRequest, cancellationToken);

            EnsureSuccess(wireResponse.ResponseHeader, "Air_TicketCancellation");

            // Air_TicketCancellation doesn't say what's refunded; the penalty Flyshop
            // applied is read back afterwards. Best-effort — the cancellation itself
            // already succeeded, so a failure here just leaves the refund unknown.
            try
            {
                var penalty = await PostAsync<AirGetCancelPenaltyRequestWire, AirGetCancelPenaltyResponseWire>(
                    "Air_APIGetCancelPenalty",
                    new AirGetCancelPenaltyRequestWire
                    {
                        AuthHeader = BuildAuthHeader(),
                        BookingRefNo = request.RefNo,
                        AirlinePnr = request.AirlinePnr
                    },
                    cancellationToken);
                var charges = penalty.CancelChargeDetails ?? new List<CancelChargeDetailWire>();
                return new SupplierCancellationResultDto(
                    charges.Count == 0 ? null : charges.Sum(c => Math.Max(0m, c.TicketFare - c.CancellationCharges)));
            }
            catch
            {
                return new SupplierCancellationResultDto(null);
            }
        }

        public async Task<SupplierBookingDetailsDto> GetBookingDetailsAsync(
            string bookingRefNo, string? airlinePnr, CancellationToken cancellationToken)
        {
            var reprint = await GetReprintAsync(bookingRefNo, airlinePnr, cancellationToken);
            var pnrs = reprint.AirPnrDetails;
            var passengers = pnrs.SelectMany(p => p.PaxTicketDetails)
                .GroupBy(p => $"{p.FirstName}|{p.LastName}|{p.PaxType}")
                .Select(g => g.First())
                .ToList();
            var paxCounts = passengers.GroupBy(p => p.PaxType).ToDictionary(g => g.Key, g => g.Count());

            var flights = pnrs.SelectMany(p => p.Flights).ToList();
            var segments = flights
                .SelectMany((flight, tripIndex) => flight.Segments.Select(seg => new SupplierBookingSegmentDto(
                    tripIndex,
                    AirportCode(seg.Origin),
                    AirportCode(seg.Destination),
                    seg.AirlineCode ?? string.Empty,
                    seg.AirlineName ?? string.Empty,
                    seg.FlightNumber ?? string.Empty,
                    ParseDate(seg.DepartureDateTime),
                    ParseDate(seg.ArrivalDateTime),
                    DurationMinutes(seg.Duration))))
                .ToList();

            // Per-passenger fare details × passengers of that type, over every flight.
            var fareDetails = flights.SelectMany(f => f.Fares.Take(1)).SelectMany(f => f.FareDetails).ToList();
            decimal ForAllPax(Func<ReprintFareDetailWire, decimal> amount) =>
                fareDetails.Sum(d => amount(d) * paxCounts.GetValueOrDefault(d.PaxType, 0));
            var baseFare = ForAllPax(d => d.BasicAmount);
            var total = ForAllPax(d => d.TotalAmount);

            // Seen on UAT: a 1 adult + 1 child booking whose Fares carried only the
            // adult entry. The split would then cover only some of the passengers,
            // so it's left out rather than shown wrong.
            var pricedPaxTypes = fareDetails.Select(d => d.PaxType).ToHashSet();
            var splitIsComplete = fareDetails.Count > 0 && paxCounts.Keys.All(pricedPaxTypes.Contains);

            var rules = fareDetails
                .SelectMany(d => (d.CancellationCharges ?? new List<ReprintCancellationChargeWire>())
                    .Where(c => decimal.TryParse(c.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                    .Select(c => new SupplierCancellationRuleDto(
                        FlyshopPaxTypeLabel(c.PassengerType),
                        decimal.Parse(c.Value!, NumberStyles.Number, CultureInfo.InvariantCulture),
                        c.ValueType == 1)))
                .ToList();

            return new SupplierBookingDetailsDto(
                segments,
                passengers
                    .Select(p => new SupplierBookingPassengerDto(
                        p.Title ?? string.Empty, p.FirstName ?? string.Empty, p.LastName ?? string.Empty,
                        FlyshopPaxTypeLabel(p.PaxType)))
                    .ToList(),
                splitIsComplete ? baseFare : null,
                splitIsComplete ? total - baseFare : null,
                splitIsComplete ? total : null,
                "INR",
                rules);
        }

        // Flyshop has no "what would cancelling cost" call (Air_APIGetCancelPenalty
        // only reports a cancellation already raised), so this estimates from the
        // booking's own cancellation rules: per passenger type, the highest charge
        // that applies (a fixed amount, or a percentage of that passenger's base
        // fare), never more than the fare. Marked as an estimate.
        public async Task<SupplierCancellationQuoteDto> GetCancellationQuoteAsync(
            SupplierCancellationQuoteRequestDto request, CancellationToken cancellationToken)
        {
            var reprint = await GetReprintAsync(request.BookingRefNo, request.AirlinePnr, cancellationToken);
            var pnrs = reprint.AirPnrDetails;
            var paxCounts = pnrs.SelectMany(p => p.PaxTicketDetails)
                .GroupBy(p => $"{p.FirstName}|{p.LastName}|{p.PaxType}")
                .Select(g => g.First())
                .GroupBy(p => p.PaxType)
                .ToDictionary(g => g.Key, g => g.Count());

            var flights = pnrs.SelectMany(p => p.Flights)
                .Where(f => request.Origin == null || f.Segments.Any(s => AirportCode(s.Origin) == request.Origin))
                .ToList();

            // A passenger type with no fare entry of its own (see GetBookingDetailsAsync)
            // is estimated at the adult entry's amounts, so every passenger is counted.
            var estimateDetails = new List<(ReprintFareDetailWire Detail, int Count)>();
            foreach (var fare in flights.SelectMany(f => f.Fares.Take(1)))
            {
                var details = fare.FareDetails;
                if (details.Count == 0)
                {
                    continue;
                }

                var proxy = details.FirstOrDefault(d => d.PaxType == 0) ?? details[0];
                foreach (var (paxType, count) in paxCounts)
                {
                    estimateDetails.Add((details.FirstOrDefault(d => d.PaxType == paxType) ?? proxy, count));
                }
            }

            decimal total = 0m, fees = 0m;
            foreach (var (detail, count) in estimateDetails)
            {
                var charge = (detail.CancellationCharges ?? new List<ReprintCancellationChargeWire>())
                    .Where(c => c.PassengerType == detail.PaxType
                        && decimal.TryParse(c.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                    .Select(c =>
                    {
                        var value = decimal.Parse(c.Value!, NumberStyles.Number, CultureInfo.InvariantCulture);
                        return c.ValueType == 1 ? detail.BasicAmount * value / 100m : value;
                    })
                    .DefaultIfEmpty(0m)
                    .Max();
                total += detail.TotalAmount * count;
                fees += Math.Min(charge, detail.TotalAmount) * count;
            }

            return new SupplierCancellationQuoteDto(total, fees, Math.Max(0m, total - fees), IsEstimate: true);
        }

        private async Task<AirReprintResponseWire> GetReprintAsync(
            string bookingRefNo, string? airlinePnr, CancellationToken cancellationToken)
        {
            var reprint = await PostAsync<AirReprintRequestWire, AirReprintResponseWire>(
                "Air_Reprint",
                new AirReprintRequestWire
                {
                    AuthHeader = BuildAuthHeader(),
                    BookingRefNo = bookingRefNo,
                    AirlinePnr = airlinePnr ?? string.Empty
                },
                cancellationToken);
            EnsureSuccess(reprint.ResponseHeader, "Air_Reprint");
            return reprint;
        }

        // "MUMBAI (BOM) " → "BOM"; a bare code is returned as-is.
        private static string AirportCode(string? value)
        {
            var text = (value ?? string.Empty).Trim();
            var open = text.LastIndexOf('(');
            var close = text.LastIndexOf(')');
            return open >= 0 && close > open ? text.Substring(open + 1, close - open - 1).Trim() : text;
        }

        private static int DurationMinutes(string? hhmm) =>
            TimeSpan.TryParse(hhmm, CultureInfo.InvariantCulture, out var span) ? (int)span.TotalMinutes : 0;

        private static string FlyshopPaxTypeLabel(int paxType) => paxType switch
        {
            1 => "Child",
            2 => "Infant",
            _ => "Adult"
        };

        public async Task ReleaseHoldAsync(
            SupplierReleaseHoldRequestDto request, CancellationToken cancellationToken)
        {
            var wireRequest = new AirReleasePnrRequestWire
            {
                AuthHeader = BuildAuthHeader(),
                BookingRefNo = request.BookingRefNo,
                AirlinePnr = request.AirlinePnr
            };

            var wireResponse = await PostAsync<AirReleasePnrRequestWire, AirReleasePnrResponseWire>(
                "Air_ReleasePNR", wireRequest, cancellationToken);

            EnsureSuccess(wireResponse.ResponseHeader, "Air_ReleasePNR");
        }

        private static SupplierAncillaryOptionDto MapSsrDetail(SsrDetailWire detail) => new(
            detail.SsrType,
            detail.SsrTypeName ?? string.Empty,
            detail.SsrTypeDesc ?? string.Empty,
            detail.SsrCode,
            detail.SsrKey ?? string.Empty,
            detail.SsrStatus,
            detail.LegIndex,
            detail.SegmentId,
            detail.SegmentWise,
            detail.TotalAmount,
            detail.CurrencyCode ?? "INR",
            detail.ApplicablePaxTypes);

        private static DateTime ParseDate(string? value) =>
            DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed
                : default;

        private AuthHeaderWire BuildAuthHeader() => new()
        {
            UserId = _options.UserId,
            Password = _options.Password,
            IpAddress = _options.IpAddress,
            RequestId = Guid.NewGuid().ToString("N"),
            ImeiNumber = _options.ImeiNumber
        };

        private static void EnsureSuccess(ResponseHeaderWire? header, string method)
        {
            if (header == null)
            {
                return;
            }

            // "0000" is the collection's documented success code; anything else carries
            // an Error_Desc worth surfacing instead of failing deserialization silently.
            if (!string.IsNullOrEmpty(header.ErrorCode) && header.ErrorCode != "0000")
            {
                var detail = string.IsNullOrEmpty(header.ErrorInnerException)
                    ? string.Empty
                    : $" ({header.ErrorInnerException})";
                throw new InvalidOperationException(
                    $"Flyshop {method} returned {header.ErrorCode}: {header.ErrorDesc}{detail}");
            }
        }

        // searchRequest is null only for Air_Reprice's own re-mapping, whose total
        // nothing in the app reads — it then keeps the single adult fare as before.
        private static SupplierFlightOptionDto MapFlight(
            FlightWire flight, int tripLegIndex = 0, FlightSearchRequestDto? searchRequest = null)
        {
            var primaryFare = flight.Fares.FirstOrDefault();

            var adultFareDetail = primaryFare?.FareDetails.FirstOrDefault(f => f.PaxType == 0)
                ?? primaryFare?.FareDetails.FirstOrDefault();

            var totalAmount = searchRequest == null
                ? adultFareDetail?.TotalAmount ?? 0m
                : TotalForPassengers(primaryFare, adultFareDetail, searchRequest);

            return new SupplierFlightOptionDto(
                flight.FlightKey,
                primaryFare?.FareId ?? string.Empty,
                flight.AirlineCode ?? flight.Segments.FirstOrDefault()?.AirlineCode ?? string.Empty,
                flight.Segments.FirstOrDefault()?.AirlineName ?? string.Empty,
                primaryFare?.Refundable ?? false,
                flight.IsLcc,
                flight.Segments.Select(MapSegment).ToList(),
                totalAmount,
                adultFareDetail?.CurrencyCode ?? "INR",
                ParseInt(primaryFare?.SeatsAvailable),
                flight.Fares.Select(f => MapFareOption(f, searchRequest)).ToList(),
                tripLegIndex);
        }

        // The offer's TotalAmount is the whole-booking price the app shows and charges,
        // so it has to cover every passenger searched for. FareDetails[].Total_Amount
        // is per passenger of that PAX_Type — confirmed live: a 2-adult search returns
        // the same adult Total_Amount as a 1-adult search for the same fare (41/41
        // unchanged flights, none doubled), while this used to return that single
        // adult fare as the whole-booking total. Fare options (MapFareOption) stay per
        // adult, matching the fare picker's "/adult" label. A pax type the fare
        // doesn't price separately falls back to the adult fare.
        // amount picks which figure is summed (the total by default, or e.g. the base fare).
        private static decimal TotalForPassengers(
            FareWire? fare, FareDetailWire? adultFareDetail, FlightSearchRequestDto request,
            Func<FareDetailWire, decimal>? amount = null)
        {
            amount ??= d => d.TotalAmount;
            var adult = adultFareDetail == null ? 0m : amount(adultFareDetail);
            decimal PerPax(int paxType)
            {
                var detail = fare?.FareDetails.FirstOrDefault(f => f.PaxType == paxType);
                return detail == null ? adult : amount(detail);
            }

            return adult * request.AdultCount
                + (request.ChildCount > 0 ? PerPax(1) * request.ChildCount : 0m)
                + (request.InfantCount > 0 ? PerPax(2) * request.InfantCount : 0m);
        }

        private static SupplierFareOptionDto MapFareOption(FareWire fare, FlightSearchRequestDto? searchRequest)
        {
            var adultFareDetail = fare.FareDetails.FirstOrDefault(f => f.PaxType == 0)
                ?? fare.FareDetails.FirstOrDefault();

            return new SupplierFareOptionDto(
                fare.FareId ?? string.Empty,
                fare.Refundable,
                adultFareDetail?.TotalAmount ?? 0m,
                adultFareDetail?.CurrencyCode ?? "INR",
                adultFareDetail?.FreeBaggage?.CheckInBaggage,
                adultFareDetail?.FreeBaggage?.HandBaggage,
                searchRequest == null ? 0m : TotalForPassengers(fare, adultFareDetail, searchRequest),
                BookingBaseAmount: searchRequest == null
                    ? 0m
                    : TotalForPassengers(fare, adultFareDetail, searchRequest, d => d.BasicAmount),
                AdultBaseFare: searchRequest == null
                    ? 0m
                    : TotalForPassengers(fare, adultFareDetail, OnePassenger(searchRequest, child: false), d => d.BasicAmount),
                ChildBaseFare: searchRequest == null
                    ? 0m
                    : TotalForPassengers(fare, adultFareDetail, OnePassenger(searchRequest, child: true), d => d.BasicAmount));
        }

        // The search request narrowed to a single adult or child, to price one passenger.
        private static FlightSearchRequestDto OnePassenger(FlightSearchRequestDto request, bool child) =>
            request with { AdultCount = child ? 0 : 1, ChildCount = child ? 1 : 0, InfantCount = 0 };

        private static SupplierFlightSegmentDto MapSegment(SegmentWire segment) => new(
            segment.Origin,
            segment.Destination,
            segment.AirlineCode,
            segment.AirlineName,
            segment.FlightNumber,
            ParseDateTime(segment.DepartureDateTime),
            ParseDateTime(segment.ArrivalDateTime),
            segment.Duration);

        // Confirmed against the live UAT sandbox: Booking_Type 3 returns "0003: No
        // flight available" for every multi-city combination tried, while 2 returns
        // real bundled itineraries (one option per Trip_Id 0 entry, each option's
        // own Segments already spanning every requested leg) — 2 is Flyshop's real
        // multi-city code, not 3.
        private static int MapBookingType(string tripType) => tripType switch
        {
            "OneWay" => 0,
            "RoundTrip" => 1,
            "MultiCity" => 2,
            _ => 0
        };

        private static string MapClassOfTravel(string cabinClass) => cabinClass switch
        {
            "Business" => "1",
            "First" => "2",
            "PremiumEconomy" => "3",
            _ => "0"
        };

        private static int ParseInt(string? value) =>
            int.TryParse(value, out var parsed) ? parsed : 0;

        private static DateTime ParseDateTime(string? value) =>
            DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed
                : default;

        private async Task<TResponse> PostAsync<TRequest, TResponse>(
            string method, TRequest body, CancellationToken cancellationToken)
        {
            using var httpResponse = await _httpClient.PostAsJsonAsync(method, body, cancellationToken);
            httpResponse.EnsureSuccessStatusCode();

            var result = await httpResponse.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);

            return result ?? throw new InvalidOperationException($"Flyshop {method} returned an empty response.");
        }
    }
}
