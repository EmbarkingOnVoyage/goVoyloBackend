using GoVoylo.Application.Common;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Commands.CreateBooking
{
    // Creates a Flyshop temp booking across every leg and immediately places a
    // Block_Ticket hold on it (see IFlightSupplierClient.CreateBlockTicketAsync
    // for why this never calls Book_Ticket). The hold is reversible via
    // Air_ReleasePNR (see IFlightSupplierClient.ReleaseHoldAsync) rather than a
    // final purchase.
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

        public CreateBookingCommandHandler(
            IFlightSupplierClientResolver supplierClientResolver,
            IFlightSearchSessionStore sessionStore,
            IUserRepository userRepository,
            IEmailService emailService,
            ITripBookingRepository tripBookingRepository,
            ISavedTravelerRepository savedTravelerRepository,
            ITravelerPassportRepository passportRepository,
            IEncryptionService encryptionService)
        {
            _supplierClientResolver = supplierClientResolver;
            _sessionStore = sessionStore;
            _userRepository = userRepository;
            _emailService = emailService;
            _tripBookingRepository = tripBookingRepository;
            _savedTravelerRepository = savedTravelerRepository;
            _passportRepository = passportRepository;
            _encryptionService = encryptionService;
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
                .Where(x => x.leg.SelectedSsrs.Count == 0)
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
                var totalAmount = legSummaries.Sum(s => s.TotalAmount);
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
                for (var i = 0; i < legSummaries.Count && i < ticket.Legs.Count; i++)
                {
                    var summary = legSummaries[i];
                    var legResult = ticket.Legs[i];

                    tripBooking.AddLeg(new TripBookingLeg(
                        tripBooking.Id,
                        i,
                        summary.Origin,
                        summary.Destination,
                        summary.TravelDate,
                        summary.AirlineCode,
                        summary.AirlineName,
                        summary.FlightNumber,
                        legResult.FlightId));
                }

                await _tripBookingRepository.AddAsync(tripBooking, cancellationToken);
            }
            catch
            {
                // Swallowed deliberately — see comment above.
            }

            return new CreateBookingResponseDto(
                ticket.BookingRefNo,
                ticket.StatusId,
                ticket.AirlineCode,
                ticket.AirlinePnr,
                ticket.CrsPnr,
                ticket.RecordLocator,
                ticket.FailureRemark);
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
                PassportExpiry = traveler.PassportExpiry ?? passport.ExpiryDate
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
