using System.Globalization;
using System.Net.Http.Json;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;

namespace GoVoylo.Infrastructure.ExternalServices.Tripjack
{
    public class TripjackClient : IFlightSupplierClient
    {
        private readonly HttpClient _httpClient;

        public TripjackClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string SupplierCode => FlightSupplierCodes.Tripjack;

        public async Task<SupplierFlightSearchResultDto> SearchAsync(
            FlightSearchRequestDto request, CancellationToken cancellationToken)
        {
            var wireRequest = new TripjackSearchRequestWire
            {
                SearchQuery = new TripjackSearchQueryWire
                {
                    CabinClass = MapCabinClass(request.CabinClass),
                    PaxInfo = new TripjackPaxInfoWire
                    {
                        Adult = request.AdultCount,
                        Child = request.ChildCount,
                        Infant = request.InfantCount
                    },
                    RouteInfos = request.Segments
                        .Select(s => new TripjackRouteInfoWire
                        {
                            FromCityOrAirport = new TripjackAirportCodeWire { Code = s.Origin },
                            ToCityOrAirport = new TripjackAirportCodeWire { Code = s.Destination },
                            TravelDate = s.TravelDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                        })
                        .ToList()
                }
            };

            var wireResponse = await PostAsync<TripjackSearchRequestWire, TripjackSearchResponseWire>(
                "fms/v1/air-search-all", wireRequest, cancellationToken);

            var tripInfos = wireResponse.SearchResult?.TripInfos ?? new Dictionary<string, List<TripjackTripOptionWire>>();

            // NOT dictionary/JSON insertion order — confirmed live that a domestic
            // return can come back with "RETURN" before "ONWARD" in the raw JSON, so
            // relying on Values' enumeration order (as this used to) silently swaps
            // leg 0 and leg 1 for exactly the itineraries this matters most for.
            // Each key is mapped to its real leg index explicitly instead — same
            // TripLegIndex meaning Flyshop's TripDetails[].Trip_Id already carries for
            // a multi-leg search.
            var flights = tripInfos
                .SelectMany(kvp => kvp.Value.Select(option => MapFlight(option, GetTripLegIndex(kvp.Key))))
                .ToList();

            // Tripjack has no equivalent of Flyshop's Search_Key — Review only ever
            // needs the priceId (carried as FlightKey/FareId below), so this is left
            // empty and never read back for a Tripjack offer.
            return new SupplierFlightSearchResultDto(string.Empty, flights);
        }

        public async Task<SupplierRepriceResultDto> RepriceAsync(
            SupplierRepriceRequestDto request, CancellationToken cancellationToken)
        {
            // Tripjack's Review is both "reprice" and "start a booking session" in one
            // call — there's no separate Flight_Key vs Fare_Id the way Flyshop has, so
            // FareId is ignored here and FlightKey (the priceId from search) is the
            // only thing Review needs.
            var wireRequest = new TripjackReviewRequestWire
            {
                PriceIds = new List<string> { request.FlightKey }
            };

            var wireResponse = await PostAsync<TripjackReviewRequestWire, TripjackReviewResponseWire>(
                "fms/v1/review", wireRequest, cancellationToken);

            if (wireResponse.Status?.Success != true || string.IsNullOrEmpty(wireResponse.BookingId))
            {
                throw new InvalidOperationException("Tripjack Review returned no bookingId.");
            }

            var totalFare = wireResponse.TotalPriceInfo?.TotalFareDetail?.FareComponent.TotalFare ?? 0m;

            // Every later Tripjack call (Seat Map, Book, Confirm-Book, Fare Rules,
            // Booking Details) needs this bookingId, not the original priceId — it's
            // carried forward as FlightKey since that's the field every caller of
            // RepriceAsync already threads through to the next step.
            return new SupplierRepriceResultDto(
                wireResponse.BookingId,
                request.FareId,
                totalFare,
                "INR",
                Repriced: true,
                IsFareChange: false);
        }

        public async Task<IReadOnlyList<SupplierRepriceResultDto>> RepriceBatchAsync(
            IReadOnlyList<SupplierRepriceRequestDto> requests, CancellationToken cancellationToken)
        {
            // Per Tripjack's own docs: a Domestic Return needs 2 priceIds at Review,
            // a Domestic Multi-City needs N (up to 6), and either way Review returns
            // ONE bookingId covering every leg together — not one bookingId per leg
            // the way RepriceAsync's single-priceId call works for a oneway. That
            // single bookingId is returned here as EVERY result's FlightKey, so
            // CreateTempBookingAsync (which only ever reads Flights[0].FlightKey) and
            // CreateBookingCommandHandler's per-leg bookkeeping both keep working
            // unmodified — they just all happen to see the same value. The combined
            // total fare is attached to the FIRST result only (0 on the rest) since
            // CreateBookingCommandHandler sums every leg's TotalAmount into the
            // booking's stored total — reporting the full combined fare on every leg
            // would multiply it by the leg count. Confirmed live: a real 2-priceId
            // domestic-return Review call returned one bookingId with
            // totalPriceInfo.totalFareDetail.fC.TF exactly equal to the sum of both
            // legs' individual fares (8824.5 + 9074.5 = 17899.0). Booking (Hold) with
            // that combined bookingId was not itself re-tested here, but reuses the
            // same oneway Book path unchanged (see CreateTempBookingAsync's own
            // comment) which is independently verified.
            if (requests.Count == 1)
            {
                return new List<SupplierRepriceResultDto> { await RepriceAsync(requests[0], cancellationToken) };
            }

            var wireRequest = new TripjackReviewRequestWire
            {
                PriceIds = requests.Select(r => r.FlightKey).ToList()
            };

            var wireResponse = await PostAsync<TripjackReviewRequestWire, TripjackReviewResponseWire>(
                "fms/v1/review", wireRequest, cancellationToken);

            if (wireResponse.Status?.Success != true || string.IsNullOrEmpty(wireResponse.BookingId))
            {
                throw new InvalidOperationException("Tripjack Review returned no bookingId.");
            }

            var totalFare = wireResponse.TotalPriceInfo?.TotalFareDetail?.FareComponent.TotalFare ?? 0m;

            return requests
                .Select((r, index) => new SupplierRepriceResultDto(
                    wireResponse.BookingId,
                    r.FareId,
                    index == 0 ? totalFare : 0m,
                    "INR",
                    Repriced: true,
                    IsFareChange: false))
                .ToList();
        }

        public Task<SupplierLowFareResultDto> GetLowFareCalendarAsync(
            SupplierLowFareRequestDto request, CancellationToken cancellationToken)
        {
            // Confirmed absent from Tripjack's Flights API v2.0 docs — no fare
            // calendar endpoint exists for this supplier at all.
            throw new NotSupportedException("Tripjack does not support a low-fare calendar.");
        }

        public Task<SupplierAncillaryResultDto> GetAncillariesAsync(
            SupplierAncillaryRequestDto request, CancellationToken cancellationToken)
        {
            // Real endpoint exists (Review's own sI[].ssrInfo already returns MEAL/
            // BAGGAGE options inline — confirmed live), but nothing here maps that
            // response into SupplierAncillaryResultDto yet.
            throw new NotSupportedException("Tripjack ancillary services are not implemented.");
        }

        public Task<SupplierSeatMapResultDto> GetSeatMapAsync(
            SupplierSeatMapRequestDto request, CancellationToken cancellationToken)
        {
            // Real endpoint: POST fms/v1/seat, { bookingId }. See
            // TripjackWireModels.cs's own notes — not yet implemented/verified.
            throw new NotSupportedException("Tripjack seat map is not implemented.");
        }

        public async Task<SupplierTempBookingResultDto> CreateTempBookingAsync(
            SupplierTempBookingRequestDto request, CancellationToken cancellationToken)
        {
            // Every leg carries the SAME bookingId here — CreateBookingCommandHandler
            // reprices multi-leg itineraries through RepriceBatchAsync (one Review
            // call, one combined bookingId returned for every leg; see its own doc
            // comment), so Flights[0].FlightKey already covers the whole itinerary
            // regardless of leg count. No multi-leg guard needed here any more.

            // RepriceAsync (Review) already ran for this leg and its bookingId is
            // carried here as FlightKey — see RepriceAsync's own doc comment.
            var bookingId = request.Flights[0].FlightKey;

            var wireRequest = new TripjackBookRequestWire
            {
                BookingId = bookingId,
                DeliveryInfo = new TripjackDeliveryInfoWire
                {
                    Emails = new List<string> { request.PassengerEmail },
                    Contacts = new List<string> { NormalizeMobile(request.PassengerMobile) }
                },
                // Sent unconditionally — see TripjackContactInfoWire's own doc
                // comment for why (nothing upstream collects a genuinely separate
                // emergency contact yet, so this doubles as the passenger's own).
                ContactInfo = new TripjackContactInfoWire
                {
                    Emails = new List<string> { request.PassengerEmail },
                    Contacts = new List<string> { NormalizeMobile(request.PassengerMobile) },
                    Ecn = $"{request.Travelers.FirstOrDefault()?.FirstName} {request.Travelers.FirstOrDefault()?.LastName}".Trim()
                },
                TravellerInfo = request.Travelers.Select(MapTraveller).ToList(),
                GstInfo = MapGstInfo(request)
                // PaymentInfos intentionally omitted — this is what makes it a Hold
                // rather than an Instant Book.
            };

            var wireResponse = await PostAsync<TripjackBookRequestWire, TripjackBookResponseWire>(
                "oms/v1/air/book", wireRequest, cancellationToken);

            EnsureSuccess(wireResponse.Status, wireResponse.Errors, "Book");

            return new SupplierTempBookingResultDto(wireResponse.BookingId ?? bookingId);
        }

        public async Task<SupplierTicketingResultDto> CreateBlockTicketAsync(
            string bookingRefNo, CancellationToken cancellationToken)
        {
            // Tripjack's own integration guide: Booking Details has to be called
            // "after 5 seconds elapsed" — the PNR/ticket data isn't populated in the
            // Book response itself, confirmed live (Book's own response is just an
            // echo of bookingId + status).
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            var wireResponse = await FetchBookingDetailsAsync(bookingRefNo, cancellationToken);

            return MapTicketingResult(bookingRefNo, wireResponse);
        }

        public async Task CancelBookingAsync(
            SupplierCancellationRequestDto request, CancellationToken cancellationToken)
        {
            // Built from Tripjack's documented 3-step amendment flow (submit ->
            // poll), NOT live-verified — this commits a real cancellation+refund even
            // on the UAT sandbox, which automated testing in this environment isn't
            // allowed to trigger. Full-booking cancel only — see this file's own
            // "Cancellation — a three-call amendment flow" comment block for why
            // trips[]/travellers[] scoping is intentionally left out.
            var submitResponse = await PostAsync<TripjackAmendmentRequestWire, TripjackSubmitAmendmentResponseWire>(
                "oms/v1/air/amendment/submit-amendment",
                new TripjackAmendmentRequestWire
                {
                    BookingId = request.RefNo,
                    Type = "CANCELLATION",
                    Remarks = request.ReqRemarks
                },
                cancellationToken);

            EnsureSuccess(submitResponse.Status, submitResponse.Errors, "Submit-Amendment");

            var amendmentId = submitResponse.AmendmentId
                ?? throw new InvalidOperationException(
                    $"Tripjack Submit-Amendment for booking '{request.RefNo}' returned no amendmentId.");

            // Tripjack's own docs: poll 4-5x, 10s apart, until the status leaves
            // REQUESTED/PENDING — contact their support if it never does.
            for (var attempt = 1; attempt <= 5; attempt++)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

                var detailsResponse = await PostAsync<TripjackAmendmentDetailsRequestWire, TripjackAmendmentDetailsResponseWire>(
                    "oms/v1/air/amendment/amendment-details",
                    new TripjackAmendmentDetailsRequestWire { AmendmentId = amendmentId },
                    cancellationToken);

                EnsureSuccess(detailsResponse.Status, detailsResponse.Errors, "Amendment-Details");

                if (detailsResponse.AmendmentStatus == "SUCCESS")
                {
                    return;
                }

                if (detailsResponse.AmendmentStatus == "REJECTED")
                {
                    throw new InvalidOperationException(
                        $"Tripjack rejected the cancellation for booking '{request.RefNo}' (amendmentId: {amendmentId}).");
                }

                // REQUESTED / PENDING — keep polling.
            }

            throw new InvalidOperationException(
                $"Tripjack cancellation for booking '{request.RefNo}' (amendmentId: {amendmentId}) is still processing " +
                "after 5 polls — contact Tripjack support per their own docs rather than retrying automatically.");
        }

        public async Task ReleaseHoldAsync(
            SupplierReleaseHoldRequestDto request, CancellationToken cancellationToken)
        {
            // Confirmed live: bookingId alone is rejected (errCode 1072,
            // "Cancellation not available for PNR") — the pnrs array is required too.
            var wireResponse = await PostAsync<TripjackUnholdRequestWire, TripjackStatusOnlyResponseWire>(
                "oms/v1/air/unhold",
                new TripjackUnholdRequestWire
                {
                    BookingId = request.BookingRefNo,
                    Pnrs = new List<string> { request.AirlinePnr }
                },
                cancellationToken);

            EnsureSuccess(wireResponse.Status, wireResponse.Errors, "Unhold");
        }

        public async Task<SupplierPaymentResultDto> AddPaymentAsync(
            string bookingRefNo, string clientRefNo, CancellationToken cancellationToken)
        {
            // Tripjack has no separate wallet-debit call the way Flyshop's AddPayment
            // is — Confirm-Book (see BookTicketAsync) takes paymentInfos.amount
            // directly and commits payment and ticketing together in one call, so no
            // money moves here. What this call CAN and must still do is fail loudly
            // before the caller treats payment as cleared: VerifyRazorpayPaymentCommand
            // Handler calls AddPaymentAsync then BookTicketAsync with no rollback in
            // between, so a hold that's already expired/cancelled on Tripjack's side
            // needs to surface here, not as a confusing Confirm-Book failure after the
            // customer's Razorpay charge is already marked succeeded.
            //
            // This maps onto Tripjack's own "Confirm Fare Before Ticketing" step
            // (their integration guide's own booking-flow diagram: Hold -> Confirm
            // Fare Before Ticketing -> [fare available?] -> Confirm-Book), a real
            // supplier-side revalidation rather than just re-reading our own cached
            // order status. Confirmed live against a real Hold: POST with just
            // { bookingId } returns the same shared status/errors envelope every
            // other endpoint uses (no extra fields), matching
            // TripjackStatusOnlyResponseWire exactly.
            var validateResponse = await PostAsync<TripjackBookingDetailsRequestWire, TripjackStatusOnlyResponseWire>(
                "oms/v1/air/fare-validate",
                new TripjackBookingDetailsRequestWire { BookingId = bookingRefNo },
                cancellationToken);

            EnsureSuccess(validateResponse.Status, validateResponse.Errors, "Fare-Validate");

            var details = await FetchBookingDetailsAsync(bookingRefNo, cancellationToken);
            var status = details.Order?.Status;

            if (status != "ON_HOLD" && status != "PENDING")
            {
                throw new InvalidOperationException(
                    $"Tripjack booking '{bookingRefNo}' is not payable (order status: {status ?? "unknown"}).");
            }

            return new SupplierPaymentResultDto(details.Order?.Amount ?? 0m, clientRefNo, "11");
        }

        public async Task<SupplierTicketingResultDto> BookTicketAsync(
            string bookingRefNo, CancellationToken cancellationToken)
        {
            // Built from Tripjack's documented Confirm-Book contract (same request
            // shape as Book, always carrying paymentInfos — see
            // TripjackConfirmBookRequestWire's own doc comment) and a real Hold +
            // Booking Details response, but this specific call was never itself run
            // live: it commits real payment+ticketing even against the UAT sandbox,
            // which automated testing in this environment isn't allowed to trigger.
            // deliveryInfo/travellerInfo are re-read from Booking Details rather than
            // threaded through this method's bookingRefNo-only signature — confirmed
            // live that Booking Details echoes both back from the original Book call.
            var details = await FetchBookingDetailsAsync(bookingRefNo, cancellationToken);
            var order = details.Order
                ?? throw new InvalidOperationException($"Tripjack booking '{bookingRefNo}' has no order details.");

            var travellerInfo = (details.ItemInfos?.Air?.TravellerInfos ?? new List<TripjackBookingTravellerInfoWire>())
                .Select(t => new TripjackTravellerInfoWire
                {
                    Title = t.Title ?? string.Empty,
                    PaxType = t.PaxType ?? "ADULT",
                    FirstName = t.FirstName ?? string.Empty,
                    LastName = t.LastName ?? string.Empty,
                    DateOfBirth = t.DateOfBirth,
                    PassportNumber = t.PassportNumber,
                    PassportExpiry = t.PassportExpiry,
                    PassportNationality = t.PassportNationality,
                    PassportIssueDate = t.PassportIssueDate,
                    PanNumber = t.PanNumber,
                    DocumentId = t.DocumentId
                })
                .ToList();

            var deliveryInfo = order.DeliveryInfo ?? new TripjackDeliveryInfoWire();

            var confirmRequest = new TripjackConfirmBookRequestWire
            {
                BookingId = bookingRefNo,
                DeliveryInfo = deliveryInfo,
                ContactInfo = new TripjackContactInfoWire
                {
                    Emails = deliveryInfo.Emails,
                    Contacts = deliveryInfo.Contacts,
                    Ecn = $"{travellerInfo.FirstOrDefault()?.FirstName} {travellerInfo.FirstOrDefault()?.LastName}".Trim()
                },
                TravellerInfo = travellerInfo,
                PaymentInfos = new List<TripjackPaymentInfoWire> { new() { Amount = order.Amount } },
                // Best-effort resend — see TripjackOrderWire.GstInfo's own doc
                // comment on why this isn't confirmed to actually round-trip.
                GstInfo = order.GstInfo
            };

            var confirmResponse = await PostAsync<TripjackConfirmBookRequestWire, TripjackBookResponseWire>(
                "oms/v1/air/confirm-book", confirmRequest, cancellationToken);

            EnsureSuccess(confirmResponse.Status, confirmResponse.Errors, "Confirm-Book");

            // Same reasoning as CreateBlockTicketAsync's own delay — the ticketed
            // PNR/status only shows up in Booking Details, not Confirm-Book's own
            // response.
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            var finalDetails = await FetchBookingDetailsAsync(bookingRefNo, cancellationToken);

            return MapTicketingResult(bookingRefNo, finalDetails);
        }

        public Task<SupplierFareRuleResultDto> GetFareRulesAsync(
            SupplierFareRuleRequestDto request, CancellationToken cancellationToken)
        {
            // Real endpoint: POST fms/v2/farerule, { flowType: "REVIEW", id: bookingId }.
            throw new NotSupportedException("Tripjack fare rules are not implemented.");
        }

        // Confirmed live: "ONWARD"/"RETURN" (domestic return), "COMBO" (international
        // return/multi-city, a single already-combined key), and plain numeric string
        // keys "0".."5" (domestic multi-city, 2-6 legs, confirmed already in
        // ascending order in the one live sample seen) — never assume dictionary/JSON
        // order reflects leg order.
        private static int GetTripLegIndex(string tripInfoKey) => tripInfoKey switch
        {
            "ONWARD" => 0,
            "RETURN" => 1,
            "COMBO" => 0,
            _ => int.TryParse(tripInfoKey, out var index) ? index : 0
        };

        private static SupplierFlightOptionDto MapFlight(TripjackTripOptionWire tripOption, int tripLegIndex)
        {
            var primaryPrice = tripOption.TotalPriceList.FirstOrDefault();
            var adultFare = GetAdultFareDetail(primaryPrice);

            var fares = tripOption.TotalPriceList
                .Select(price =>
                {
                    var fareDetail = GetAdultFareDetail(price);
                    return new SupplierFareOptionDto(
                        price.Id,
                        fareDetail?.RefundableType != 0,
                        fareDetail?.FareComponent.TotalFare ?? 0m,
                        "INR",
                        fareDetail?.BaggageInfo?.CheckInBaggage,
                        fareDetail?.BaggageInfo?.CabinBaggage);
                })
                .ToList();

            var firstSegment = tripOption.SegmentInfos.FirstOrDefault();

            return new SupplierFlightOptionDto(
                // No separate Flight_Key/Fare_Id split for Tripjack — the priceId
                // covers both (see TripjackPriceWire.Id's own doc comment).
                primaryPrice?.Id ?? string.Empty,
                primaryPrice?.Id ?? string.Empty,
                firstSegment?.FlightDesignator.AirlineInfo.Code ?? string.Empty,
                firstSegment?.FlightDesignator.AirlineInfo.Name ?? string.Empty,
                adultFare?.RefundableType != 0,
                firstSegment?.FlightDesignator.AirlineInfo.IsLcc ?? false,
                tripOption.SegmentInfos.Select(MapSegment).ToList(),
                adultFare?.FareComponent.TotalFare ?? 0m,
                "INR",
                adultFare?.SeatsRemaining ?? 0,
                fares,
                tripLegIndex);
        }

        private static TripjackFareDetailWire? GetAdultFareDetail(TripjackPriceWire? price) =>
            price?.FareDetailsByPaxType.TryGetValue("ADULT", out var detail) == true ? detail : null;

        private static SupplierFlightSegmentDto MapSegment(TripjackSegmentInfoWire segment) => new(
            segment.Departure.Code,
            segment.Arrival.Code,
            segment.FlightDesignator.AirlineInfo.Code,
            segment.FlightDesignator.AirlineInfo.Name,
            segment.FlightDesignator.FlightNumber,
            ParseDateTime(segment.DepartureDateTime),
            ParseDateTime(segment.ArrivalDateTime),
            segment.DurationMinutes.ToString(CultureInfo.InvariantCulture));

        private Task<TripjackBookingDetailsResponseWire> FetchBookingDetailsAsync(
            string bookingRefNo, CancellationToken cancellationToken) =>
            PostAsync<TripjackBookingDetailsRequestWire, TripjackBookingDetailsResponseWire>(
                "oms/v1/booking-details",
                new TripjackBookingDetailsRequestWire { BookingId = bookingRefNo },
                cancellationToken);

        // Shared by CreateBlockTicketAsync (after Book) and BookTicketAsync (after
        // Confirm-Book) — both end with the same "read back whatever Booking Details
        // now shows" step, just at different points in the hold -> pay -> ticket flow.
        private static SupplierTicketingResultDto MapTicketingResult(
            string bookingRefNo, TripjackBookingDetailsResponseWire wireResponse)
        {
            var order = wireResponse.Order;
            var airInfo = wireResponse.ItemInfos?.Air;
            var traveller = airInfo?.TravellerInfos.FirstOrDefault();
            var pnrDetails = traveller?.PnrDetails ?? new Dictionary<string, string>();
            var statusId = MapOrderStatus(order?.Status);

            var legs = (airInfo?.TripInfos ?? new List<TripjackTripOptionWire>())
                .SelectMany(t => t.SegmentInfos)
                .Select(seg =>
                {
                    // No single stable per-segment id the way Flyshop's Flight_Id is
                    // (Booking Details' own segment id changes between Search/Review/
                    // Book, confirmed live) — the route itself is the one identifier
                    // that stays meaningful across calls, and it's also exactly how
                    // pnrDetails keys its entries ("DEP-ARR"), so it doubles as the
                    // lookup key for that route's own PNR. A oneway has one entry; a
                    // roundtrip/multi-city can have a different PNR per route (same
                    // reasoning already applied to Flyshop's own per-leg AirlinePnr).
                    var flightId = $"{seg.Departure.Code}-{seg.Arrival.Code}";
                    pnrDetails.TryGetValue(flightId, out var legPnr);

                    return new SupplierTicketingLegResultDto(
                        flightId,
                        statusId,
                        seg.FlightDesignator.AirlineInfo.Code,
                        legPnr,
                        null,
                        null,
                        null);
                })
                .ToList();

            // First leg's PNR, kept for existing single-leg callers — see
            // SupplierTicketingResultDto's own doc comment.
            var firstPnr = legs.FirstOrDefault()?.AirlinePnr;

            return new SupplierTicketingResultDto(
                order?.BookingId ?? bookingRefNo,
                statusId,
                legs.FirstOrDefault()?.AirlineCode,
                firstPnr,
                null,
                null,
                null,
                legs);
        }

        // Tripjack's own Order Status values, mapped onto the same "11-Success/
        // 22-Failed/33-Block" string convention Flyshop's Status_Id already uses —
        // every existing handler (CancelTripBookingCommandHandler,
        // VerifyRazorpayPaymentCommandHandler, etc.) branches on those literal
        // strings, so reusing them here means a Tripjack booking works with that
        // logic unchanged rather than needing every call site to become
        // supplier-aware about status semantics too.
        private static string MapOrderStatus(string? status) => status switch
        {
            "SUCCESS" => "11",
            "ON_HOLD" => "33",
            "PENDING" => "33",
            _ => "22"
        };

        private static TripjackTravellerInfoWire MapTraveller(SupplierTempBookingPaxDto pax) => new()
        {
            Title = MapTitle(pax.PaxType, pax.Gender),
            PaxType = MapPaxType(pax.PaxType),
            FirstName = pax.FirstName,
            LastName = pax.LastName,
            DateOfBirth = pax.DateOfBirth?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            PassportNumber = pax.PassportNumber,
            PassportExpiry = pax.PassportExpiry?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            PassportNationality = pax.PassportNationality,
            PassportIssueDate = pax.PassportIssueDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            PanNumber = pax.PanNumber,
            DocumentId = pax.DocumentId
        };

        // GstInfo is booking-level (one invoice per booking), not per-traveler — see
        // SupplierTempBookingRequestDto.Gst*. RegisteredName/Address fall back to
        // empty rather than null since Tripjack's own field table marks them
        // required whenever gstInfo itself is sent.
        private static TripjackGstInfoWire? MapGstInfo(SupplierTempBookingRequestDto request) =>
            request.Gst
                ? new TripjackGstInfoWire
                {
                    GstNumber = request.GstNumber,
                    RegisteredName = request.GstHolderName,
                    Address = request.GstAddress,
                    Email = request.PassengerEmail,
                    Mobile = NormalizeMobile(request.PassengerMobile)
                }
                : null;

        // 0-ADT/1-CHD/2-INF, the same convention SupplierTempBookingPaxDto's own
        // callers already use for Flyshop.
        private static string MapPaxType(int paxType) => paxType switch
        {
            1 => "CHILD",
            2 => "INFANT",
            _ => "ADULT"
        };

        // Tripjack's own docs: Adult titles are Mr/Mrs/Ms, Child/Infant titles are
        // Ms/Master — Gender (0-Male/1-Female) is all SupplierTempBookingPaxDto
        // carries, so a female adult maps to Mrs rather than distinguishing Ms.
        private static string MapTitle(int paxType, int gender)
        {
            var isFemale = gender == 1;
            return paxType switch
            {
                1 or 2 => isFemale ? "Ms" : "Master",
                _ => isFemale ? "Mrs" : "Mr"
            };
        }

        // Tripjack's docs: "contact numbers with country code e.g. +919500112233".
        private static string NormalizeMobile(string mobile) =>
            mobile.StartsWith('+') ? mobile : $"+91{mobile}";

        private static void EnsureSuccess(TripjackStatusWire? status, List<TripjackErrorWire>? errors, string method)
        {
            if (status?.Success == true)
            {
                return;
            }

            var detail = errors == null || errors.Count == 0
                ? string.Empty
                : $" ({string.Join("; ", errors.Select(e => $"{e.ErrorCode}: {e.Message}"))})";

            throw new InvalidOperationException($"Tripjack {method} failed{detail}");
        }

        private static string MapCabinClass(string cabinClass) => cabinClass switch
        {
            "Business" => "BUSINESS",
            "First" => "FIRST",
            "PremiumEconomy" => "PREMIUM_ECONOMY",
            _ => "ECONOMY"
        };

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

            return result ?? throw new InvalidOperationException($"Tripjack {method} returned an empty response.");
        }
    }
}
