using GoVoylo.Application.Common;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Domain.Pricing;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Commands.CreateBooking
{
    // Creates a supplier temp booking across every leg, held until payment where
    // the supplier holds (see IFlightSupplierClient.CreateBlockTicketAsync) — never
    // a final purchase. Book_Ticket runs only once payment verifies (see
    // TripBookingTicketingService).
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, CreateBookingResponseDto>
    {
        // Air_Ticketing's own docs: Status_Id 22 is the only failure code —
        // everything else (11-Success, 33-Block) means the hold/ticket went through.
        private const string TicketingFailedStatusId = "22";

        private readonly IFlightSupplierClientResolver _supplierClientResolver;
        private readonly IFlightSearchSessionStore _sessionStore;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly ITripBookingRepository _tripBookingRepository;
        private readonly ISavedTravelerRepository _savedTravelerRepository;
        private readonly ITravelerPassportRepository _passportRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly IConvenienceFeeService _convenienceFeeService;

        public CreateBookingCommandHandler(
            IFlightSupplierClientResolver supplierClientResolver,
            IFlightSearchSessionStore sessionStore,
            IUserRepository userRepository,
            IEmailService emailService,
            ITripBookingRepository tripBookingRepository,
            ISavedTravelerRepository savedTravelerRepository,
            ITravelerPassportRepository passportRepository,
            IEncryptionService encryptionService,
            IConvenienceFeeService convenienceFeeService)
        {
            _supplierClientResolver = supplierClientResolver;
            _sessionStore = sessionStore;
            _userRepository = userRepository;
            _emailService = emailService;
            _tripBookingRepository = tripBookingRepository;
            _savedTravelerRepository = savedTravelerRepository;
            _passportRepository = passportRepository;
            _encryptionService = encryptionService;
            _convenienceFeeService = convenienceFeeService;
        }

        public async Task<CreateBookingResponseDto> Handle(
            CreateBookingCommand request, CancellationToken cancellationToken)
        {
            // Route/price display data for each leg, captured from the original search
            // session (Reprice only ever changes FlightKey/FareId) — kept around purely
            // to persist a TripBooking once ticketing succeeds, below.
            var legSummaries = new List<FlightOfferSession>();

            foreach (var leg in request.Legs)
            {
                var session = await _sessionStore.GetAsync(leg.OfferId, cancellationToken);

                if (session == null)
                {
                    throw new NotFoundException("Flight offer not found or has expired. Please search again.");
                }

                legSummaries.Add(session);
            }

            // Every leg has to come from the same supplier — a mixed-supplier
            // itinerary (one leg via Flyshop, another via Tripjack) isn't something
            // any single Book_Ticket-style call can commit atomically. Checked before
            // repricing (not just before CreateTempBookingAsync, as this used to)
            // since a batched reprice call below is itself only valid for one
            // supplier at a time.
            var supplierCode = legSummaries[0].SupplierCode;
            if (legSummaries.Any(s => s.SupplierCode != supplierCode))
            {
                throw new BusinessRuleException(
                    "mixed_supplier_booking",
                    "All legs of a booking must come from the same supplier.");
            }

            var supplierClient = _supplierClientResolver.Resolve(supplierCode);

            // A leg can book one of its offer's other fares (fare picker, or the
            // matched fare of a supplier special-return package) rather than the
            // default. That fare must be priced afresh — the session may already
            // hold a reprice of the default fare (Tripjack: a bookingId from
            // Review) — so the leg is forced through reprice below. Tripjack
            // reprices by the price id itself, which is its fare id, so it becomes
            // the leg's FlightKey too; Flyshop reprices by Flight_Key + Fare_Id.
            var fareChangedLegs = new HashSet<int>();
            for (var i = 0; i < request.Legs.Count; i++)
            {
                var requestedFareId = request.Legs[i].FareId;
                if (string.IsNullOrWhiteSpace(requestedFareId) || requestedFareId == legSummaries[i].FareId)
                {
                    continue;
                }

                legSummaries[i] = legSummaries[i] with
                {
                    FareId = requestedFareId,
                    FlightKey = supplierCode == FlightSupplierCodes.Tripjack ? requestedFareId : legSummaries[i].FlightKey
                };
                fareChangedLegs.Add(i);
            }

            // Worked out before anything is booked with the supplier, from the base
            // fares and trip type the search session recorded server-side.
            var convenienceFee = await CalculateConvenienceFeeAsync(request, legSummaries, cancellationToken);

            // A seat/meal/baggage SSR_Key returned by Air_GetSeatMap or Air_GetSSR is
            // only valid against the exact Flight_Key that request was repriced
            // against (both handlers persist their reprice back into the session).
            // Repricing again here — as this used to do unconditionally — hands
            // Air_TempBooking a *different* Flight_Key than the one the SSR_Key was
            // issued for, and the seat lock in particular doesn't carry over: the
            // booking still creates a Ref_No, but Air_Ticketing then fails at the
            // airline host (confirmed live: "Err002: This transaction is already
            // Rejected/Deleted"). A plain meal SSR tolerated the mismatch in testing,
            // but seat selection consistently did not, so any selection is treated
            // the same way here — when the caller already selected SSRs for this leg,
            // trust the session's current Flight_Key/Fare_Id (already fresh from
            // whichever ancillaries/seatmap call produced those keys) instead of
            // repricing again. A leg with no SSR selections still reprices as before,
            // since its Flight_Key may still be the original, un-repriced search
            // result (Air_TempBooking's own docs require a Reprice-sourced Flight_Key).
            //
            // Every leg needing reprice goes through ONE RepriceBatchAsync call rather
            // than one RepriceAsync call each — Flyshop's own implementation just runs
            // them in a loop internally (identical behavior/result to before), but
            // Tripjack's Review needs every leg's priceId submitted together to get a
            // single bookingId covering the whole itinerary (see
            // IFlightSupplierClient.RepriceBatchAsync's own doc comment) — a
            // roundtrip/multi-city Tripjack booking isn't possible without this.
            var legsNeedingReprice = request.Legs
                .Select((leg, index) => (leg, index))
                .Where(x => x.leg.SelectedSsrs.Count == 0 || fareChangedLegs.Contains(x.index))
                .ToList();

            if (legsNeedingReprice.Count > 0)
            {
                var repriceRequests = legsNeedingReprice
                    .Select(x => new SupplierRepriceRequestDto(
                        legSummaries[x.index].SearchKey, legSummaries[x.index].FlightKey, legSummaries[x.index].FareId))
                    .ToList();

                var repriceResults = await supplierClient.RepriceBatchAsync(repriceRequests, cancellationToken);

                for (var i = 0; i < legsNeedingReprice.Count; i++)
                {
                    var index = legsNeedingReprice[i].index;
                    var result = repriceResults[i];

                    var updatedSession = legSummaries[index] with
                    {
                        FlightKey = result.FlightKey,
                        FareId = result.FareId
                    };
                    await _sessionStore.UpdateAsync(request.Legs[index].OfferId, updatedSession, cancellationToken);

                    legSummaries[index] = updatedSession;
                }
            }

            var bookingFlights = request.Legs
                .Select((leg, index) => new SupplierBookingFlightDto(
                    legSummaries[index].SearchKey,
                    legSummaries[index].FlightKey,
                    leg.SelectedSsrs
                        .Select(s => new SupplierBookingSsrDto(s.PaxId, s.SsrKey))
                        .ToList()))
                .ToList();

            var requestTravelers = new List<BookingTravelerRequestDto>(request.Travelers.Count);
            foreach (var traveler in request.Travelers)
            {
                requestTravelers.Add(await WithSavedPassportAsync(traveler, request.UserId));
            }

            var travelers = requestTravelers
                .Select(t => new SupplierTempBookingPaxDto(
                    t.PaxId,
                    MapPaxType(t.PaxType),
                    t.Title,
                    t.FirstName,
                    t.LastName,
                    MapGender(t.Gender),
                    t.DateOfBirth,
                    t.PassportNumber,
                    t.PassportNationality,
                    t.PassportExpiry,
                    t.PassportIssueDate,
                    t.PanNumber,
                    t.DocumentId))
                .ToList();

            var hasGst = !string.IsNullOrWhiteSpace(request.GstNumber);

            var tempBooking = await supplierClient.CreateTempBookingAsync(
                new SupplierTempBookingRequestDto(
                    request.PassengerMobile,
                    request.PassengerEmail,
                    travelers,
                    bookingFlights,
                    Gst: hasGst,
                    GstNumber: request.GstNumber ?? string.Empty,
                    GstHolderName: request.GstHolderName ?? string.Empty,
                    GstAddress: request.GstAddress ?? string.Empty),
                cancellationToken);

            var ticket = await supplierClient.CreateBlockTicketAsync(tempBooking.BookingRefNo, cancellationToken);

            // Best-effort: the hold itself already succeeded on Flyshop's side by this
            // point, so a failed/missing email shouldn't turn a successful booking
            // response into an error for the caller — just skip sending rather than
            // throw. The user's registered account email is used here, deliberately
            // separate from PassengerEmail (the trip's own contact email).
            if (ticket.StatusId != TicketingFailedStatusId)
            {
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (!string.IsNullOrWhiteSpace(user?.Email))
                {
                    try
                    {
                        await _emailService.SendBookingConfirmationAsync(
                            user.Email,
                            $"{user.FirstName} {user.LastName}".Trim(),
                            ticket.BookingRefNo,
                            ticket.AirlinePnr,
                            ticket.RecordLocator);
                    }
                    catch
                    {
                        // Swallowed deliberately — see comment above.
                    }
                }
            }

            // Best-effort, same reasoning as the email above: persistence failing
            // shouldn't turn an already-successful Flyshop hold/ticket into an error
            // response — the customer still has a real booking even if it doesn't show
            // up in "My Trips" this one time.
            try
            {
                var totalAmount = ticket.ConfirmedTotalAmount ?? legSummaries.Sum(s => s.TotalAmount);
                var currencyCode = legSummaries.FirstOrDefault()?.CurrencyCode ?? "INR";
                var passengerNames = string.Join(", ", request.Travelers.Select(t => $"{t.FirstName} {t.LastName}"));
                var paxIds = string.Join(",", request.Travelers.Select(t => t.PaxId));

                var tripBooking = new TripBooking(
                    request.UserId,
                    supplierCode,
                    ticket.BookingRefNo,
                    ticket.AirlinePnr,
                    ticket.CrsPnr,
                    ticket.RecordLocator,
                    ticket.StatusId,
                    totalAmount,
                    currencyCode,
                    passengerNames,
                    paxIds);

                tripBooking.SetConvenienceFee(convenienceFee);

                if (tempBooking.DeferredBookPayload != null)
                {
                    tripBooking.SetDeferredSupplierPayload(
                        _encryptionService.Encrypt(tempBooking.DeferredBookPayload));
                }

                // Zipped by index: Air_Ticketing's AirlinePNRDetails is expected to
                // preserve the order Air_TempBooking's BookingFlightDetails was sent in.
                // If Flyshop ever returns a different count, the shorter list wins and
                // any unmatched leg is skipped — a missing leg's FlightId means it can't
                // be individually cancelled later, but that's strictly better than the
                // booking not appearing in "My Trips" at all.
                //
                // One requested leg = one saved TripBookingLeg, using that leg's own
                // Origin/Destination (first/last segment) even when the leg itself
                // spans multiple physical flights — confirmed live against a
                // combined-itinerary (SPECIALROUNDTRIP/Booking_Type 2) booking that
                // Air_Ticketing returns exactly one AirlinePNRDetails entry per
                // requested leg regardless of its physical segment count, i.e.
                // Flyshop tracks it as a single connecting-flight-style PNR, not as
                // independently cancellable segments. Splitting per physical segment
                // here would misrepresent that PNR as ending at its first stop.
                //
                // A whole-trip offer (one offer for a Tripjack international return or
                // multi-city) is saved as one leg per trip instead — its own
                // Origin/Destination would otherwise read DEL-DEL. Tripjack's ticketing
                // legs are per segment, keyed "DEP-ARR", so each trip takes the next
                // one departing from its origin.
                if (legSummaries.Count == 1 && legSummaries[0].Trips is { Count: > 1 } trips)
                {
                    var remainingLegs = ticket.Legs.ToList();
                    for (var i = 0; i < trips.Count; i++)
                    {
                        var trip = trips[i];
                        var legResult = remainingLegs.FirstOrDefault(l => l.FlightId.StartsWith($"{trip.Origin}-", StringComparison.Ordinal));
                        if (legResult != null)
                        {
                            remainingLegs.Remove(legResult);
                        }

                        var tripLeg = new TripBookingLeg(
                            tripBooking.Id,
                            i,
                            trip.Origin,
                            trip.Destination,
                            trip.TravelDate,
                            trip.AirlineCode,
                            trip.AirlineName,
                            trip.FlightNumber,
                            legResult?.FlightId ?? string.Empty);
                        // One whole-trip offer, so every trip shares its one fare.
                        tripLeg.SetFareType(request.Legs[0].FareType);
                        tripBooking.AddLeg(tripLeg);
                    }
                }
                else
                {
                    // No legs back at all means nothing is ticketed or held yet
                    // (Flyshop's temp booking awaiting payment) — every leg is saved
                    // without a FlightId, filled in once Air_Ticketing runs.
                    var legCount = ticket.Legs.Count == 0
                        ? legSummaries.Count
                        : Math.Min(legSummaries.Count, ticket.Legs.Count);
                    for (var i = 0; i < legCount; i++)
                    {
                        var summary = legSummaries[i];
                        var legResult = ticket.Legs.ElementAtOrDefault(i);

                        var leg = new TripBookingLeg(
                            tripBooking.Id,
                            i,
                            summary.Origin,
                            summary.Destination,
                            summary.TravelDate,
                            summary.AirlineCode,
                            summary.AirlineName,
                            summary.FlightNumber,
                            legResult?.FlightId ?? string.Empty);
                        leg.SetFareType(i < request.Legs.Count ? request.Legs[i].FareType : null);
                        tripBooking.AddLeg(leg);
                    }
                }

                await _tripBookingRepository.AddAsync(tripBooking, cancellationToken);
            }
            catch when (tempBooking.DeferredBookPayload == null)
            {
                // Swallowed deliberately — see comment above. Not for a deferred
                // (non-holdable) booking, though: nothing is booked with the supplier
                // yet and the saved record is the only copy of the request needed
                // after payment, so failing to save it must fail before payment.
            }

            return new CreateBookingResponseDto(
                ticket.BookingRefNo,
                ticket.StatusId,
                ticket.AirlineCode,
                ticket.AirlinePnr,
                ticket.CrsPnr,
                ticket.RecordLocator,
                ticket.FailureRemark,
                ticket.ConfirmedTotalAmount,
                convenienceFee);
        }

        // One journey per leg, or per trip of a whole-trip offer (its base fare split
        // evenly across trips, since the supplier prices the offer as one). The trip
        // type is RoundTrip only for two journeys found by a round-trip search; any
        // other multi-journey booking is MultiCity — the app searches multi-city
        // legs one at a time, so those sessions say OneWay.
        private async Task<decimal> CalculateConvenienceFeeAsync(
            CreateBookingCommand request, IReadOnlyList<FlightOfferSession> legSummaries, CancellationToken cancellationToken)
        {
            var journeys = new List<JourneyBaseFare>();
            for (var i = 0; i < legSummaries.Count; i++)
            {
                var session = legSummaries[i];
                var fareId = request.Legs[i].FareId ?? session.FareId;
                var fare = session.Fares?.FirstOrDefault(f => f.FareId == fareId)
                    ?? session.Fares?.FirstOrDefault(f => f.FareId == session.FareId);
                var tripCount = Math.Max(1, session.Trips?.Count ?? 1);
                for (var t = 0; t < tripCount; t++)
                {
                    journeys.Add(new JourneyBaseFare(
                        (fare?.AdultBaseFare ?? 0m) / tripCount,
                        (fare?.ChildBaseFare ?? 0m) / tripCount));
                }
            }

            var tripType = journeys.Count == 1
                ? TripTypes.OneWay
                : journeys.Count == 2 && legSummaries.Any(s => s.TripType == TripTypes.RoundTrip)
                    ? TripTypes.RoundTrip
                    : TripTypes.MultiCity;

            return await _convenienceFeeService.CalculateAsync(
                tripType,
                journeys,
                request.Travelers.Count(t => MapPaxType(t.PaxType) == 0),
                request.Travelers.Count(t => MapPaxType(t.PaxType) == 1),
                cancellationToken);
        }

        // Fills passport details from a saved co-traveller's passport on file — see
        // BookingTravelerRequestDto.SavedTravelerId. Explicit passport fields on the
        // request always win; a traveller with no passport on file is passed through
        // unchanged (domestic bookings don't need one).
        private async Task<BookingTravelerRequestDto> WithSavedPassportAsync(
            BookingTravelerRequestDto traveler, Guid userId)
        {
            if (traveler.SavedTravelerId is not { } savedTravelerId
                || !string.IsNullOrWhiteSpace(traveler.PassportNumber))
            {
                return traveler;
            }

            var savedTraveler = await _savedTravelerRepository.GetByIdAsync(savedTravelerId);
            if (savedTraveler == null || savedTraveler.UserId != userId)
            {
                throw new NotFoundException("Traveler not found.");
            }

            var passport = await _passportRepository.GetByTravelerIdAsync(savedTravelerId);
            if (passport == null)
            {
                return traveler;
            }

            return traveler with
            {
                PassportNumber = _encryptionService.Decrypt(passport.PassportNumberEncrypted),
                PassportNationality = string.IsNullOrWhiteSpace(traveler.PassportNationality)
                    ? CountryCodes.ToIso2(passport.IssuingCountry)
                    : traveler.PassportNationality,
                PassportExpiry = traveler.PassportExpiry ?? passport.ExpiryDate,
                PassportIssueDate = traveler.PassportIssueDate ?? passport.IssueDate
            };
        }

        // 0-ADT/1-CHD/2-INF, matching Flyshop's own FareDetails.PAX_Type convention.
        private static int MapPaxType(string paxType) => paxType switch
        {
            "Child" => 1,
            "Infant" => 2,
            _ => 0
        };

        // 0-Male/1-Female — the only two values Flyshop's PAX_Details documents.
        private static int MapGender(string gender) => gender switch
        {
            "Female" => 1,
            _ => 0
        };
    }
}
