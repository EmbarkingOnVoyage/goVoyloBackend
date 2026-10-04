using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using Microsoft.Extensions.Caching.Memory;

namespace GoVoylo.Infrastructure.ExternalServices.Tripjack
{
    public class TripjackClient : IFlightSupplierClient
    {
        // Review is the only place Tripjack exposes ssrInfo pre-booking (no separate
        // "get SSR options" call the way Flyshop's Air_GetSSR is, and a bookingId
        // can't be re-submitted to Review to fetch it again later — confirmed live,
        // errCode 808 "Keys Passed in the request is already expired"). Every
        // RepriceAsync/RepriceBatchAsync call caches its own Review response's
        // tripInfos here, keyed by bookingId, so GetAncillariesAsync (called right
        // after a reprice, per GetFlightAncillariesQueryHandler's own generic
        // sequencing) can read it back without a second supplier call. Same 15-minute
        // TTL already used for InMemoryFlightSearchSessionStore, in the same spirit
        // (roughly matches Review's own conditions.st session time).
        private static readonly TimeSpan SsrCacheTtl = TimeSpan.FromMinutes(15);

        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public TripjackClient(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
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
                .SelectMany(kvp => kvp.Value.Select(option => MapFlight(option, GetTripLegIndex(kvp.Key), request)))
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
            var priceIds = new List<string> { request.FlightKey };
            if (IsBookingId(request.FlightKey))
            {
                if (!IsBookingUsed(request.FlightKey))
                {
                    return ReuseReviewedBooking(request.FlightKey, request.FareId);
                }

                priceIds = OriginalPriceIds(request.FlightKey).ToList();
            }

            var wireRequest = new TripjackReviewRequestWire
            {
                PriceIds = priceIds
            };

            var wireResponse = await PostAsync<TripjackReviewRequestWire, TripjackReviewResponseWire>(
                "fms/v1/review", wireRequest, cancellationToken);

            if (wireResponse.Status?.Success != true || string.IsNullOrEmpty(wireResponse.BookingId))
            {
                throw new InvalidOperationException("Tripjack Review returned no bookingId.");
            }

            var totalFare = wireResponse.TotalPriceInfo?.TotalFareDetail?.FareComponent.TotalFare ?? 0m;

            CacheSsrTripInfos(wireResponse.BookingId, wireResponse.TripInfos);
            CacheReviewedFare(wireResponse.BookingId, totalFare, wireResponse.Conditions?.IsHoldAllowed ?? true, priceIds);

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

            // Every leg of an already-reviewed itinerary carries the same combined
            // bookingId (see above) — reuse it rather than re-Reviewing. A mix of
            // reviewed and un-reviewed legs can't be combined into one Review call.
            var priceIds = requests.Select(r => r.FlightKey).ToList();
            if (requests.Any(r => IsBookingId(r.FlightKey)))
            {
                var bookingId = requests[0].FlightKey;
                if (requests.Any(r => r.FlightKey != bookingId))
                {
                    throw new InvalidOperationException(
                        "Tripjack itinerary legs have diverged across review sessions. Please search again.");
                }

                if (IsBookingUsed(bookingId))
                {
                    priceIds = OriginalPriceIds(bookingId).ToList();
                }
                else
                {
                    var reused = ReuseReviewedBooking(bookingId, requests[0].FareId);
                    return requests
                        .Select((r, index) => reused with
                        {
                            FareId = r.FareId,
                            TotalAmount = index == 0 ? reused.TotalAmount : 0m
                        })
                        .ToList();
                }
            }

            var wireRequest = new TripjackReviewRequestWire
            {
                PriceIds = priceIds
            };

            var wireResponse = await PostAsync<TripjackReviewRequestWire, TripjackReviewResponseWire>(
                "fms/v1/review", wireRequest, cancellationToken);

            if (wireResponse.Status?.Success != true || string.IsNullOrEmpty(wireResponse.BookingId))
            {
                throw new InvalidOperationException("Tripjack Review returned no bookingId.");
            }

            var totalFare = wireResponse.TotalPriceInfo?.TotalFareDetail?.FareComponent.TotalFare ?? 0m;

            CacheSsrTripInfos(wireResponse.BookingId, wireResponse.TripInfos);
            CacheReviewedFare(wireResponse.BookingId, totalFare, wireResponse.Conditions?.IsHoldAllowed ?? true, priceIds);

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
            // request.FlightKey is a bookingId by this point — GetFlightAncillaries
            // QueryHandler always calls RepriceAsync immediately before this, and
            // Tripjack's Review response converts the priceId into a bookingId (see
            // RepriceAsync's own doc comment). The ssrInfo needed here only ever
            // appears in that same Review response, cached by CacheSsrTripInfos — see
            // this class's own SsrCacheTtl doc comment for why a second live call
            // can't recover it (bookingId isn't a valid Review input).
            if (!_cache.TryGetValue(SsrCacheKey(request.FlightKey), out List<TripjackTripOptionWire>? tripInfos)
                || tripInfos == null)
            {
                throw new InvalidOperationException(
                    $"No cached Tripjack SSR data for bookingId '{request.FlightKey}' — the reprice that should " +
                    "have preceded this call is missing or its cache entry expired.");
            }

            var options = tripInfos
                .SelectMany((trip, legIndex) => trip.SegmentInfos.SelectMany(seg => MapSsrOptions(seg, legIndex)))
                .ToList();

            return Task.FromResult(new SupplierAncillaryResultDto(options));
        }

        private static IEnumerable<SupplierAncillaryOptionDto> MapSsrOptions(
            TripjackSegmentInfoWire segment, int legIndex)
        {
            var segmentId = segment.Id ?? string.Empty;
            // The real identity lives in SsrKey's own string segment id — this int is
            // only for SupplierAncillaryOptionDto's shared shape (originally
            // Flyshop's own numeric Segment_Id) and isn't read back at Book time.
            var segmentIdInt = int.TryParse(segmentId, out var parsed) ? parsed : 0;

            return MapSsrCategory("BAGGAGE", segment.SsrInfo?.Baggage, segmentId, segmentIdInt, legIndex)
                .Concat(MapSsrCategory("MEAL", segment.SsrInfo?.Meal, segmentId, segmentIdInt, legIndex))
                .Concat(MapSsrCategory("EXTRASERVICES", segment.SsrInfo?.ExtraServices, segmentId, segmentIdInt, legIndex));
        }

        private static IEnumerable<SupplierAncillaryOptionDto> MapSsrCategory(
            string category, List<TripjackSsrOptionWire>? items, string segmentId, int segmentIdInt, int legIndex) =>
            (items ?? new List<TripjackSsrOptionWire>()).Select(item => new SupplierAncillaryOptionDto(
                MapSsrCategoryType(category),
                category,
                item.Desc,
                item.Code,
                // Encodes everything CreateTempBookingAsync's own MapTraveller needs
                // to rebuild a TripjackSsrSelectionWire at Book time without a second
                // Tripjack call — category picks which ssrXxxInfos list to populate,
                // segmentId is the key, code is the code.
                $"{category}:{segmentId}:{item.Code}",
                // Meaningless for Tripjack's baggage/meal/extra-service options (no
                // equivalent of Flyshop's seat-only 0-ISLE/1-AVAILABLE/etc. states) —
                // always 0, same convention Flyshop's own MapSsrDetail already uses
                // for its non-seat options.
                0,
                legIndex,
                segmentIdInt,
                SegmentWise: true,
                item.Amount ?? 0m,
                "INR",
                // Tripjack's ssrInfo items carry no pax-type restriction field the way
                // Flyshop's own SSR details do — empty means "not further restricted"
                // rather than "restricted to nothing", consistent with how
                // GetFlightAncillariesQueryHandler passes this straight through
                // without ever filtering on it itself.
                Array.Empty<int>()));

        // Must match Flyshop's own SSR_Type numbering (0-Baggage, 1-Meals,
        // 2-Complimentary meals, 3-Seat), which the app filters its Baggage/Meal
        // lists on. Extra services have no Flyshop equivalent, so they get a value
        // outside that range and stay out of both lists.
        private static int MapSsrCategoryType(string category) => category switch
        {
            "BAGGAGE" => 0,
            "MEAL" => 1,
            "SEAT" => 3,
            "EXTRASERVICES" => 99,
            _ => 99
        };

        private void CacheSsrTripInfos(string bookingId, List<TripjackTripOptionWire> tripInfos)
        {
            if (string.IsNullOrEmpty(bookingId) || tripInfos.Count == 0)
            {
                return;
            }

            _cache.Set(SsrCacheKey(bookingId), tripInfos, SsrCacheTtl);
        }

        private static string SsrCacheKey(string bookingId) => $"tripjack-ssr:{bookingId}";

        // Review hands back a bookingId that every later call threads through as
        // FlightKey — but callers built around Flyshop's model (GetSeatMap, and
        // CreateBooking for a leg with no SSR selections) reprice again first, and
        // Tripjack rejects a bookingId sent back to Review (errCode 808 "Keys Passed
        // in the request is already expired", confirmed live on a real booking
        // attempt). An already-reviewed bookingId is still valid for Seat Map/Book,
        // so it's reused as-is with the fare cached from the original Review.
        // Search priceIds look like "11-6776401094_0DELBOMAI2678~...", bookingIds
        // always "TJS..." (every bookingId in Tripjack's own sample logs).
        private static bool IsBookingId(string flightKey) =>
            flightKey.StartsWith("TJS", StringComparison.Ordinal);

        // HoldAllowed is Review's conditions.isBA — see CreateTempBookingAsync for
        // what happens when it's false. PriceIds are the search priceIds this
        // bookingId was reviewed from, kept so a bookingId that's already been
        // booked can be reviewed afresh (see IsBookingUsed).
        private sealed record ReviewedFare(decimal TotalFare, bool HoldAllowed, IReadOnlyList<string> PriceIds);

        private void CacheReviewedFare(string bookingId, decimal totalFare, bool holdAllowed, IReadOnlyList<string> priceIds) =>
            _cache.Set(ReviewedFareCacheKey(bookingId), new ReviewedFare(totalFare, holdAllowed, priceIds), SsrCacheTtl);

        // A bookingId can be booked only once. After a hold is released (the
        // customer cancelled checkout) the offer session still carries it, and
        // booking it again fails with errCode 2714 "Duplicate booking Id" (confirmed
        // live). Marked here once Book succeeds, so the next reprice reviews the
        // original priceIds again for a fresh bookingId.
        private void MarkBookingUsed(string bookingId) =>
            _cache.Set(BookingUsedCacheKey(bookingId), true, SsrCacheTtl);

        private bool IsBookingUsed(string bookingId) =>
            _cache.TryGetValue(BookingUsedCacheKey(bookingId), out bool used) && used;

        private IReadOnlyList<string> OriginalPriceIds(string bookingId) =>
            _cache.TryGetValue(ReviewedFareCacheKey(bookingId), out ReviewedFare? reviewed) && reviewed != null
                ? reviewed.PriceIds
                : throw new InvalidOperationException("This Tripjack fare session has expired. Please search again.");

        private static string BookingUsedCacheKey(string bookingId) => $"tripjack-booked:{bookingId}";

        private SupplierRepriceResultDto ReuseReviewedBooking(string bookingId, string fareId)
        {
            if (!_cache.TryGetValue(ReviewedFareCacheKey(bookingId), out ReviewedFare? reviewed) || reviewed == null)
            {
                throw new InvalidOperationException(
                    "This Tripjack fare session has expired. Please search again.");
            }

            return new SupplierRepriceResultDto(
                bookingId, fareId, reviewed.TotalFare, "INR", Repriced: true, IsFareChange: false);
        }

        private static string ReviewedFareCacheKey(string bookingId) => $"tripjack-review-fare:{bookingId}";

        // Seat prices from the last Seat Map call per bookingId, keyed
        // "segmentId:seatCode" — only needed to price a deferred instant booking
        // (see CreateTempBookingAsync), since Review's ssrInfo doesn't cover seats.
        private static string SeatPriceCacheKey(string bookingId) => $"tripjack-seat-prices:{bookingId}";

        // Set by CreateTempBookingAsync when it deferred the booking instead of
        // holding, so CreateBlockTicketAsync (called right after) knows there's no
        // Tripjack booking to look up yet.
        private static string DeferredCacheKey(string bookingId) => $"tripjack-deferred:{bookingId}";

        private static readonly JsonSerializerOptions PayloadJsonOptions = new(JsonSerializerDefaults.Web);

        private static TripjackBookRequestWire DeserializeDeferredBook(string payload) =>
            JsonSerializer.Deserialize<TripjackBookRequestWire>(payload, PayloadJsonOptions)
            ?? throw new InvalidOperationException("Deferred Tripjack booking payload is empty.");

        public async Task<SupplierSeatMapResultDto> GetSeatMapAsync(
            SupplierSeatMapRequestDto request, CancellationToken cancellationToken)
        {
            // request.FlightKey is a bookingId by this point — GetSeatMapQueryHandler
            // always calls RepriceAsync immediately before this, same as
            // GetAncillariesAsync's own doc comment explains.
            var wireResponse = await PostAsync<TripjackSeatMapRequestWire, TripjackSeatMapResponseWire>(
                "fms/v1/seat",
                new TripjackSeatMapRequestWire { BookingId = request.FlightKey },
                cancellationToken);

            EnsureSuccess(wireResponse.Status, wireResponse.Errors, "Seat Map");

            // Seat Map's own response has no leg/trip grouping of its own — just a
            // flat dictionary keyed by segment id. The cached Review tripInfos
            // (populated by the RepriceAsync call that always precedes this one — see
            // GetAncillariesAsync's own SsrCacheKey doc comment) is reused purely to
            // look up which leg each segment id belongs to.
            _cache.TryGetValue(SsrCacheKey(request.FlightKey), out List<TripjackTripOptionWire>? tripInfos);
            var legIndexBySegmentId = (tripInfos ?? new List<TripjackTripOptionWire>())
                .SelectMany((trip, legIndex) => trip.SegmentInfos.Select(seg => (seg.Id, legIndex)))
                .Where(x => x.Id != null)
                .ToDictionary(x => x.Id!, x => x.legIndex);

            var segments = (wireResponse.TripSeatMap?.TripSeat ?? new Dictionary<string, TripjackSegmentSeatMapWire>())
                .Select(kvp =>
                {
                    var legIndex = legIndexBySegmentId.GetValueOrDefault(kvp.Key, 0);

                    // Response is a flat sInfo list with seatPosition.row/column —
                    // unlike Flyshop's own Air_GetSeatMap, which already comes
                    // pre-grouped into rows — so the grouping happens here.
                    var rows = (kvp.Value.Seats ?? new List<TripjackSeatWire>())
                        .GroupBy(s => s.SeatPosition.Row)
                        .OrderBy(g => g.Key)
                        .Select(g => new SupplierSeatRowDto(
                            g.OrderBy(s => s.SeatPosition.Column)
                                .Select(s => MapSeat(s, kvp.Key, legIndex))
                                .ToList()))
                        .ToList();

                    return new SupplierSeatSegmentDto(legIndex, rows);
                })
                .ToList();

            var seatPrices = (wireResponse.TripSeatMap?.TripSeat ?? new Dictionary<string, TripjackSegmentSeatMapWire>())
                .SelectMany(kvp => (kvp.Value.Seats ?? new List<TripjackSeatWire>())
                    .Select(s => (Key: $"{kvp.Key}:{s.Code}", s.Amount)))
                .GroupBy(x => x.Key)
                .ToDictionary(g => g.Key, g => g.First().Amount);
            _cache.Set(SeatPriceCacheKey(request.FlightKey), seatPrices, SsrCacheTtl);

            return new SupplierSeatMapResultDto(segments);
        }

        private static SupplierAncillaryOptionDto MapSeat(TripjackSeatWire seat, string segmentId, int legIndex) => new(
            MapSsrCategoryType("SEAT"),
            "SEAT",
            seat.SeatNo,
            seat.Code,
            // Decoded by MapTraveller into ssrSeatInfos at Book time, same
            // "category:segmentId:code" scheme as GetAncillariesAsync's own
            // MapSsrCategory.
            $"SEAT:{segmentId}:{seat.Code}",
            // 0-ISLE/1-AVAILABLE/2-BLOCKED/3-BOOKED, Flyshop's own convention (see
            // SupplierAncillaryOptionDto's own doc comment) — Tripjack only
            // distinguishes booked/available, so this collapses to those two.
            seat.IsBooked ? 3 : 1,
            legIndex,
            int.TryParse(segmentId, out var segmentIdInt) ? segmentIdInt : 0,
            SegmentWise: true,
            seat.Amount,
            "INR",
            Array.Empty<int>());

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

            // A leg with SSRs skips repricing (see CreateBookingCommandHandler), so a
            // used bookingId can still reach here. Its SSR keys carry that Review's
            // segment ids, so it can't be silently re-reviewed; the customer has to
            // pick the flight again.
            if (IsBookingUsed(bookingId))
            {
                throw new BusinessRuleException(
                    "fare_session_used",
                    "This fare was already used for an earlier booking attempt. Please select the flight again.");
            }

            // Defensive: every leg SHOULD carry the same bookingId by this point, but
            // CreateBookingCommandHandler skips repricing any leg that already has
            // SSRs selected (trusting its session's existing FlightKey — see that
            // handler's own comment) rather than always batching every leg together.
            // For a multi-leg Tripjack itinerary where SSRs were selected on only
            // SOME legs, that could leave one leg holding an older/different
            // bookingId than the rest (from whenever its ancillaries were fetched)
            // while the others get a fresh one from RepriceBatchAsync — silently
            // booking against the wrong reviewed itinerary rather than a loud error.
            // Not yet reworked to prevent this at the source (would mean forcing a
            // full re-reprice for every leg whenever ANY leg needs one, for Tripjack
            // specifically) — this at least turns a silent mismatch into a clear one.
            if (request.Flights.Any(f => f.FlightKey != bookingId))
            {
                throw new InvalidOperationException(
                    "Tripjack booking legs carry different bookingIds — likely a mix of SSR-selected and " +
                    "un-selected legs on a multi-leg itinerary caused some legs to skip repricing. Reselect SSRs " +
                    "for every leg (or none) and try again.");
            }

            // Every leg's SelectedSsrs are gathered by PaxId (not just Flights[0]'s) —
            // a multi-leg Tripjack itinerary shares one bookingId, but a traveler can
            // still have picked different SSRs on different legs (different segment
            // ids), and Tripjack expects all of a traveler's selections together in
            // their own travellerInfo entry rather than split by leg the way
            // Flyshop's BookingFlightDetails[].BookingSsrDetails is.
            var ssrSelectionsByPaxId = request.Flights
                .SelectMany(f => f.SelectedSsrs)
                .GroupBy(s => s.PaxId)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<SupplierBookingSsrDto>)g.ToList());

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
                TravellerInfo = request.Travelers
                    .Select(t => MapTraveller(t, ssrSelectionsByPaxId.GetValueOrDefault(t.PaxId)))
                    .ToList(),
                GstInfo = MapGstInfo(request)
                // PaymentInfos intentionally omitted — this is what makes it a Hold
                // rather than an Instant Book.
            };

            // A fare Review marked not holdable (conditions.isBA false) rejects a
            // Hold outright — confirmed live: errCode 1087 "Blocking is not allowed
            // for this Booking". Nothing is booked with Tripjack yet in that case:
            // the same request, with paymentInfos, is handed back to be persisted
            // and sent as an instant booking once the customer's payment is
            // verified (see BookTicketAsync). The amount has to match Tripjack's own
            // total, so selected SSRs/seats are priced in from the cached Review/Seat
            // Map responses.
            if (_cache.TryGetValue(ReviewedFareCacheKey(bookingId), out ReviewedFare? reviewed)
                && reviewed is { HoldAllowed: false })
            {
                var ssrTotal = PriceSsrSelections(bookingId, ssrSelectionsByPaxId.Values.SelectMany(s => s));
                wireRequest.PaymentInfos = new List<TripjackPaymentInfoWire>
                {
                    new() { Amount = reviewed.TotalFare + ssrTotal }
                };

                _cache.Set(DeferredCacheKey(bookingId), reviewed.TotalFare + ssrTotal, SsrCacheTtl);

                return new SupplierTempBookingResultDto(
                    bookingId, JsonSerializer.Serialize(wireRequest, PayloadJsonOptions));
            }

            var wireResponse = await PostAsync<TripjackBookRequestWire, TripjackBookResponseWire>(
                "oms/v1/air/book", wireRequest, cancellationToken);

            EnsureSuccess(wireResponse.Status, wireResponse.Errors, "Book");
            MarkBookingUsed(bookingId);

            return new SupplierTempBookingResultDto(wireResponse.BookingId ?? bookingId);
        }

        // Prices "category:segmentId:code" SsrKeys (see MapTraveller) from the cached
        // Review ssrInfo, or the cached Seat Map for seats. A selection that can't be
        // priced fails loudly rather than under-paying and having Tripjack reject the
        // instant booking after the customer has already paid.
        private decimal PriceSsrSelections(string bookingId, IEnumerable<SupplierBookingSsrDto> selections)
        {
            _cache.TryGetValue(SsrCacheKey(bookingId), out List<TripjackTripOptionWire>? tripInfos);
            _cache.TryGetValue(SeatPriceCacheKey(bookingId), out Dictionary<string, decimal>? seatPrices);

            var segmentsById = (tripInfos ?? new List<TripjackTripOptionWire>())
                .SelectMany(t => t.SegmentInfos)
                .Where(s => s.Id != null)
                .GroupBy(s => s.Id!)
                .ToDictionary(g => g.Key, g => g.First());

            var total = 0m;
            foreach (var selection in selections)
            {
                var parts = selection.SsrKey.Split(':', 3);
                if (parts.Length != 3)
                {
                    throw new InvalidOperationException($"Unrecognised Tripjack SSR key '{selection.SsrKey}'.");
                }

                var (category, segmentId, code) = (parts[0], parts[1], parts[2]);

                decimal? amount;
                if (category == "SEAT")
                {
                    amount = seatPrices != null && seatPrices.TryGetValue($"{segmentId}:{code}", out var seatPrice)
                        ? seatPrice
                        : null;
                }
                else
                {
                    var ssrInfo = segmentsById.GetValueOrDefault(segmentId)?.SsrInfo;
                    var options = category switch
                    {
                        "BAGGAGE" => ssrInfo?.Baggage,
                        "MEAL" => ssrInfo?.Meal,
                        "EXTRASERVICES" => ssrInfo?.ExtraServices,
                        _ => null
                    };
                    var option = options?.FirstOrDefault(o => o.Code == code);
                    amount = option == null ? null : option.Amount ?? 0m;
                }

                total += amount ?? throw new InvalidOperationException(
                    "The selected add-on prices have expired. Please search again.");
            }

            return total;
        }

        public async Task<SupplierTicketingResultDto> CreateBlockTicketAsync(
            string bookingRefNo, CancellationToken cancellationToken)
        {
            // Deferred (non-holdable) fare — see CreateTempBookingAsync. Nothing
            // exists on Tripjack's side to look up yet, so the result is built from
            // the cached Review itinerary with Status 33 ("held", i.e. awaiting
            // payment), which is what VerifyRazorpayPaymentCommandHandler expects
            // before it makes the real booking. Leg ids follow MapTicketingResult's
            // own "DEP-ARR" route scheme so they line up once ticketed.
            if (_cache.TryGetValue(DeferredCacheKey(bookingRefNo), out decimal deferredAmount))
            {
                _cache.TryGetValue(SsrCacheKey(bookingRefNo), out List<TripjackTripOptionWire>? tripInfos);
                var pendingLegs = (tripInfos ?? new List<TripjackTripOptionWire>())
                    .SelectMany(t => t.SegmentInfos)
                    .Select(seg => new SupplierTicketingLegResultDto(
                        $"{seg.Departure.Code}-{seg.Arrival.Code}",
                        "33",
                        seg.FlightDesignator.AirlineInfo.Code,
                        null, null, null, null))
                    .ToList();

                return new SupplierTicketingResultDto(
                    bookingRefNo, "33", pendingLegs.FirstOrDefault()?.AirlineCode, null, null, null, null, pendingLegs,
                    ConfirmedTotalAmount: deferredAmount);
            }

            // Tripjack's own integration guide: Booking Details has to be called
            // "after 5 seconds elapsed" — the PNR/ticket data isn't populated in the
            // Book response itself, confirmed live (Book's own response is just an
            // echo of bookingId + status).
            //
            // A Hold only counts once the order reads ON_HOLD. It can sit at PENDING
            // while the airline confirms it, and Confirm-Book is refused against a
            // PENDING order (errCode 2520 "Invalid Action Requested for current
            // Order Status") — confirmed live on a paid international multi-city
            // booking. So this polls while PENDING, and a hold still unconfirmed
            // afterwards is reported as failed BEFORE the customer is charged.
            TripjackBookingDetailsResponseWire wireResponse;
            var polls = 0;
            do
            {
                await Task.Delay(TicketingPollInterval, cancellationToken);
                wireResponse = await FetchBookingDetailsAsync(bookingRefNo, cancellationToken);
                polls++;
            }
            while (wireResponse.Order?.Status == "PENDING" && polls < TicketingMaxPolls);

            var result = MapTicketingResult(bookingRefNo, wireResponse);
            if (wireResponse.Order?.Status != "PENDING")
            {
                return result;
            }

            return result with
            {
                StatusId = "22",
                FailureRemark = "The airline hasn't confirmed the hold on this fare yet. Please try again.",
                Legs = result.Legs.Select(l => l with { StatusId = "22" }).ToList()
            };
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
            string bookingRefNo, string clientRefNo, string? deferredBookPayload, CancellationToken cancellationToken)
        {
            // Deferred (non-holdable) fare: there's no hold to validate yet — the
            // instant booking in BookTicketAsync is itself Tripjack's fare check, and
            // fails with Tripjack's own error if the fare is no longer available.
            if (deferredBookPayload != null)
            {
                var deferred = DeserializeDeferredBook(deferredBookPayload);
                return new SupplierPaymentResultDto(
                    deferred.PaymentInfos?.Sum(p => p.Amount) ?? 0m, clientRefNo, "11");
            }

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

            // Only a confirmed hold is payable — Confirm-Book refuses a PENDING order
            // (see CreateBlockTicketAsync).
            if (status != "ON_HOLD")
            {
                throw new InvalidOperationException(
                    $"Tripjack booking '{bookingRefNo}' is not payable (order status: {status ?? "unknown"}).");
            }

            return new SupplierPaymentResultDto(details.Order?.Amount ?? 0m, clientRefNo, "11");
        }

        public async Task<SupplierTicketingResultDto> BookTicketAsync(
            string bookingRefNo, string? deferredBookPayload, CancellationToken cancellationToken)
        {
            // Deferred (non-holdable) fare — see CreateTempBookingAsync. Sends the
            // persisted Book request (already carrying paymentInfos) as Tripjack's
            // instant booking, which books and tickets in one call.
            if (deferredBookPayload != null)
            {
                var instantBook = DeserializeDeferredBook(deferredBookPayload);

                var instantResponse = await PostAsync<TripjackBookRequestWire, TripjackBookResponseWire>(
                    "oms/v1/air/book", instantBook, cancellationToken);

                EnsureSuccess(instantResponse.Status, instantResponse.Errors, "Book (instant)");

                return await AwaitTicketingResultAsync(bookingRefNo, cancellationToken);
            }

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
                // Resent as registered at Book time (see
                // TripjackBookingDetailsResponseWire.GstInfo); a booking without GST
                // reads back an empty gstInfo, which isn't sent.
                GstInfo = string.IsNullOrWhiteSpace(details.GstInfo?.GstNumber) ? null : details.GstInfo
            };

            var confirmResponse = await PostAsync<TripjackConfirmBookRequestWire, TripjackBookResponseWire>(
                "oms/v1/air/confirm-book", confirmRequest, cancellationToken);

            EnsureSuccess(confirmResponse.Status, confirmResponse.Errors, "Confirm-Book");

            return await AwaitTicketingResultAsync(bookingRefNo, cancellationToken);
        }

        // After a paid Confirm-Book/instant Book, the ticketed PNR/status only shows
        // up in Booking Details (first read no sooner than 5s, per Tripjack's guide),
        // and the order can sit at PENDING while the airline tickets — confirmed live:
        // a paid international return read back PENDING (PNR already issued) right
        // after Confirm-Book. Polls while PENDING, then reports anything still short
        // of SUCCESS as TicketingPendingStatusId rather than the hold's "33": the
        // customer has paid, so it must not look like a releasable hold.
        private const string TicketingPendingStatusId = "44";
        private static readonly TimeSpan TicketingPollInterval = TimeSpan.FromSeconds(5);
        private const int TicketingMaxPolls = 12;

        private async Task<SupplierTicketingResultDto> AwaitTicketingResultAsync(
            string bookingRefNo, CancellationToken cancellationToken)
        {
            TripjackBookingDetailsResponseWire details;
            var polls = 0;
            do
            {
                await Task.Delay(TicketingPollInterval, cancellationToken);
                details = await FetchBookingDetailsAsync(bookingRefNo, cancellationToken);
                polls++;
            }
            while (details.Order?.Status == "PENDING" && polls < TicketingMaxPolls);

            var result = MapTicketingResult(bookingRefNo, details);
            if (result.StatusId != "33")
            {
                return result;
            }

            return result with
            {
                StatusId = TicketingPendingStatusId,
                Legs = result.Legs.Select(l => l with { StatusId = TicketingPendingStatusId }).ToList()
            };
        }

        public async Task<SupplierFareRuleResultDto> GetFareRulesAsync(
            SupplierFareRuleRequestDto request, CancellationToken cancellationToken)
        {
            // request.FlightKey is a bookingId by this point — GetFareRulesQueryHandler
            // always calls RepriceAsync immediately before this, same reasoning as
            // GetAncillariesAsync/GetSeatMapAsync's own doc comments — so this always
            // sends flowType REVIEW rather than SEARCH.
            var wireResponse = await PostAsync<TripjackFareRuleRequestWire, TripjackFareRuleResponseWire>(
                "fms/v2/farerule",
                new TripjackFareRuleRequestWire { FlowType = "REVIEW", Id = request.FlightKey },
                cancellationToken);

            EnsureSuccess(wireResponse.Status, wireResponse.Errors, "Fare Rule");

            var rules = (wireResponse.FareRule ?? new Dictionary<string, TripjackRouteFareRuleWire>())
                .SelectMany(routeEntry => (routeEntry.Value.TimedFareRule ?? new Dictionary<string, List<TripjackFareRulePolicyWire>>())
                    .Select(policyEntry => new SupplierFareRuleDto(
                        routeEntry.Key,
                        policyEntry.Key,
                        FormatFareRulePolicies(policyEntry.Value))))
                .ToList();

            return new SupplierFareRuleResultDto(rules);
        }

        // Tripjack returns structured time-banded policy data (amount/additionalFee/
        // st/et per band), not the free-text description SupplierFareRuleDto.
        // FareRuleDesc otherwise carries (Flyshop returns a full XHTML fare-rule
        // document, already stripped to plain text elsewhere) — this flattens each
        // policy type's bands into one readable summary instead of exposing the
        // structured data through this shared shape.
        private static string FormatFareRulePolicies(List<TripjackFareRulePolicyWire> policies)
        {
            var lines = policies.Select(p =>
            {
                var window = p.StartTimeHours != null && p.EndTimeHours != null
                    ? $"{p.StartTimeHours}-{p.EndTimeHours} hrs before departure: "
                    : !string.IsNullOrEmpty(p.PolicyPeriod)
                        ? $"{p.PolicyPeriod}: "
                        : string.Empty;

                var fee = p.Amount is > 0 or null && p.AdditionalFee is > 0
                    ? $"Airline fee {p.Amount ?? 0:0.##}, Tripjack fee {p.AdditionalFee ?? 0:0.##}. "
                    : p.Amount is > 0
                        ? $"Fee {p.Amount:0.##}. "
                        : string.Empty;

                var info = p.PolicyInfo ?? string.Empty;

                return $"{window}{fee}{info}".Trim();
            });

            return string.Join(" | ", lines.Where(l => l.Length > 0));
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

        private static SupplierFlightOptionDto MapFlight(
            TripjackTripOptionWire tripOption, int tripLegIndex, FlightSearchRequestDto request)
        {
            // A SPECIAL_RETURN fare is only bookable paired with a matching
            // special-return fare on the other leg — confirmed live: an onward
            // SPECIAL_RETURN picked alongside a PUBLISHED return was rejected at
            // Review with errCode 1080 "All Segments Must be selected if Special
            // Return fare". Legs are picked independently, so each offer's primary
            // (bookable-by-default) fare prefers anything else, falling back to
            // the first fare only when every fare is special-return.
            var primaryPrice = tripOption.TotalPriceList.FirstOrDefault(p => p.FareIdentifier != "SPECIAL_RETURN")
                ?? tripOption.TotalPriceList.FirstOrDefault();
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
                        fareDetail?.BaggageInfo?.CabinBaggage,
                        TotalForPassengers(price, request),
                        price.FareIdentifier,
                        price.SpecialReturnId,
                        price.MatchingSpecialReturnIds);
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
                MapSegments(tripOption.SegmentInfos),
                TotalForPassengers(primaryPrice, request),
                "INR",
                adultFare?.SeatsRemaining ?? 0,
                fares,
                tripLegIndex);
        }

        // The offer's TotalAmount is what the app shows as the trip price and charges
        // at payment, so it has to cover every passenger searched for. fd[paxType].fC.TF
        // is per passenger — confirmed live: Review's total for 2 ADT + 2 CHD was
        // exactly 4x the search's per-adult TF, while this used to return just the
        // single adult fare as the whole-booking total. The per-fare options (Fares)
        // stay per adult, matching the fare picker's "/adult" label. A pax type the
        // fare doesn't price separately falls back to the adult fare.
        private static decimal TotalForPassengers(TripjackPriceWire? price, FlightSearchRequestDto request)
        {
            var adult = GetAdultFareDetail(price)?.FareComponent.TotalFare ?? 0m;
            decimal PerPax(string paxType) =>
                price?.FareDetailsByPaxType.TryGetValue(paxType, out var detail) == true
                    ? detail.FareComponent.TotalFare
                    : adult;

            return adult * request.AdultCount
                + (request.ChildCount > 0 ? PerPax("CHILD") * request.ChildCount : 0m)
                + (request.InfantCount > 0 ? PerPax("INFANT") * request.InfantCount : 0m);
        }

        private static TripjackFareDetailWire? GetAdultFareDetail(TripjackPriceWire? price) =>
            price?.FareDetailsByPaxType.TryGetValue("ADULT", out var detail) == true ? detail : null;

        // A new trip starts wherever sN goes back to 0 (see TripjackSegmentInfoWire.SegmentNumber).
        private static List<SupplierFlightSegmentDto> MapSegments(IEnumerable<TripjackSegmentInfoWire> segments)
        {
            var tripIndex = -1;
            return segments
                .Select(segment =>
                {
                    if (segment.SegmentNumber == 0 || tripIndex < 0)
                    {
                        tripIndex++;
                    }

                    return MapSegment(segment, tripIndex);
                })
                .ToList();
        }

        private static SupplierFlightSegmentDto MapSegment(TripjackSegmentInfoWire segment, int tripIndex) => new(
            segment.Departure.Code,
            segment.Arrival.Code,
            segment.FlightDesignator.AirlineInfo.Code,
            segment.FlightDesignator.AirlineInfo.Name,
            segment.FlightDesignator.FlightNumber,
            ParseDateTime(segment.DepartureDateTime),
            ParseDateTime(segment.ArrivalDateTime),
            segment.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            tripIndex);

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
                legs,
                // Booking Details' order amount — what Tripjack charges for the
                // hold, including selected SSRs.
                ConfirmedTotalAmount: order != null && order.Amount > 0 ? order.Amount : null);
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

        private static TripjackTravellerInfoWire MapTraveller(
            SupplierTempBookingPaxDto pax, IReadOnlyList<SupplierBookingSsrDto>? ssrSelections = null)
        {
            var wire = new TripjackTravellerInfoWire
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

            // SsrKey decodes "{category}:{segmentId}:{code}" — see
            // GetAncillariesAsync's own MapSsrCategory, which encoded it.
            foreach (var selection in ssrSelections ?? Array.Empty<SupplierBookingSsrDto>())
            {
                var parts = selection.SsrKey.Split(':', 3);
                if (parts.Length != 3)
                {
                    continue;
                }

                var entry = new TripjackSsrSelectionWire { Key = parts[1], Code = parts[2] };

                switch (parts[0])
                {
                    case "BAGGAGE":
                        (wire.SsrBaggageInfos ??= new List<TripjackSsrSelectionWire>()).Add(entry);
                        break;
                    case "MEAL":
                        (wire.SsrMealInfos ??= new List<TripjackSsrSelectionWire>()).Add(entry);
                        break;
                    case "EXTRASERVICES":
                        (wire.SsrExtraServiceInfos ??= new List<TripjackSsrSelectionWire>()).Add(entry);
                        break;
                    case "SEAT":
                        (wire.SsrSeatInfos ??= new List<TripjackSsrSelectionWire>()).Add(entry);
                        break;
                }
            }

            return wire;
        }

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
