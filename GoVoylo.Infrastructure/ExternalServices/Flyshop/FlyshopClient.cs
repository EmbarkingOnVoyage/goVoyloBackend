using System.Globalization;
using System.Net.Http.Json;
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
                .SelectMany(t => t.Flights.Select(f => MapFlight(f, t.TripId ?? 0)))
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
                        Gender = t.Gender
                    })
                    .ToList(),
                Gst = false,
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

        public async Task<SupplierTicketingResultDto> CreateBlockTicketAsync(
            string bookingRefNo, CancellationToken cancellationToken)
        {
            var wireRequest = new AirTicketingRequestWire
            {
                AuthHeader = BuildAuthHeader(),
                BookingRefNo = bookingRefNo,
                TicketingType = "0"
            };

            var wireResponse = await PostAsync<AirTicketingRequestWire, AirTicketingResponseWire>(
                "Air_Ticketing", wireRequest, cancellationToken);

            EnsureSuccess(wireResponse.ResponseHeader, "Air_Ticketing");

            var detail = wireResponse.AirlinePnrDetails.FirstOrDefault();
            var pnr = detail?.AirlinePnrs.FirstOrDefault();

            return new SupplierTicketingResultDto(
                wireResponse.BookingRefNo ?? bookingRefNo,
                detail?.StatusId ?? string.Empty,
                pnr?.AirlineCode,
                pnr?.AirlinePnr,
                pnr?.RecordLocator,
                detail?.FailureRemark);
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

        private static SupplierFlightOptionDto MapFlight(FlightWire flight, int tripLegIndex = 0)
        {
            var primaryFare = flight.Fares.FirstOrDefault();

            var adultFareDetail = primaryFare?.FareDetails.FirstOrDefault(f => f.PaxType == 0)
                ?? primaryFare?.FareDetails.FirstOrDefault();

            return new SupplierFlightOptionDto(
                flight.FlightKey,
                primaryFare?.FareId ?? string.Empty,
                flight.AirlineCode ?? flight.Segments.FirstOrDefault()?.AirlineCode ?? string.Empty,
                flight.Segments.FirstOrDefault()?.AirlineName ?? string.Empty,
                primaryFare?.Refundable ?? false,
                flight.IsLcc,
                flight.Segments.Select(MapSegment).ToList(),
                adultFareDetail?.TotalAmount ?? 0m,
                adultFareDetail?.CurrencyCode ?? "INR",
                ParseInt(primaryFare?.SeatsAvailable),
                flight.Fares.Select(MapFareOption).ToList(),
                tripLegIndex);
        }

        private static SupplierFareOptionDto MapFareOption(FareWire fare)
        {
            var adultFareDetail = fare.FareDetails.FirstOrDefault(f => f.PaxType == 0)
                ?? fare.FareDetails.FirstOrDefault();

            return new SupplierFareOptionDto(
                fare.FareId ?? string.Empty,
                fare.Refundable,
                adultFareDetail?.TotalAmount ?? 0m,
                adultFareDetail?.CurrencyCode ?? "INR",
                adultFareDetail?.FreeBaggage?.CheckInBaggage,
                adultFareDetail?.FreeBaggage?.HandBaggage);
        }

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
