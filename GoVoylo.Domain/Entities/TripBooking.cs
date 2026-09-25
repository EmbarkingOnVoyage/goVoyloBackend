using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    // A persisted record of a real booking made through Flyshop (Air_TempBooking +
    // Air_Ticketing) — separate from the legacy/unused FlightBooking entity, which
    // predates the real supplier integration and isn't written to by any live code
    // path. Backs the "My Trips" list and its cancel/release action.
    public class TripBooking : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string BookingRefNo { get; private set; } = null!;
        public string? AirlinePnr { get; private set; }
        public string? CrsPnr { get; private set; }
        public string? RecordLocator { get; private set; }

        // Flyshop's own status at creation time: 11-Success/22-Failed/33-Block.
        public string StatusId { get; private set; } = null!;

        // App-tracked lifecycle, independent of StatusId: Active/Cancelled/Released.
        // Flyshop's own status never changes in our copy after creation, so this is
        // the field the "My Trips" UI actually reflects once the user acts on a trip.
        public string LocalStatus { get; private set; } = null!;

        public decimal TotalAmount { get; private set; }
        public string CurrencyCode { get; private set; } = null!;

        // Comma-joined "First Last" — display only, not used for cancellation.
        public string PassengerNames { get; private set; } = null!;

        // Comma-joined 1-based Pax_Ids (e.g. "1,2") — reused to rebuild the
        // Air_TicketCancellation segment list without asking the caller to resupply
        // Flyshop's own identifiers a second time.
        public string PaxIds { get; private set; } = null!;

        public DateTime? CancelledAt { get; private set; }

        // Only set when LocalStatus is Cancelled (via Air_TicketCancellation) — a
        // Released hold went through Air_ReleasePNR instead, which has no
        // CancellationType/CancelCode concept of its own.
        public int? CancellationType { get; private set; }
        public string? CancelCode { get; private set; }

        private readonly List<TripBookingLeg> _legs = new();
        public IReadOnlyList<TripBookingLeg> Legs => _legs;

        public const string StatusActive = "Active";
        public const string StatusCancelled = "Cancelled";
        public const string StatusReleased = "Released";

        public TripBooking(
            Guid userId,
            string bookingRefNo,
            string? airlinePnr,
            string? crsPnr,
            string? recordLocator,
            string statusId,
            decimal totalAmount,
            string currencyCode,
            string passengerNames,
            string paxIds)
        {
            UserId = userId;
            BookingRefNo = bookingRefNo;
            AirlinePnr = airlinePnr;
            CrsPnr = crsPnr;
            RecordLocator = recordLocator;
            StatusId = statusId;
            LocalStatus = StatusActive;
            TotalAmount = totalAmount;
            CurrencyCode = currencyCode;
            PassengerNames = passengerNames;
            PaxIds = paxIds;
        }

        // Required by EF Core
        private TripBooking()
        {
        }

        public void AddLeg(TripBookingLeg leg) => _legs.Add(leg);

        // Populates the in-memory Legs collection when reading back from storage —
        // TripBookingLeg has no EF navigation back to TripBooking (this codebase
        // queries child tables separately rather than via Include), so the
        // repository loads legs itself and calls this to assemble the aggregate.
        public void LoadLegs(IEnumerable<TripBookingLeg> legs)
        {
            _legs.Clear();
            _legs.AddRange(legs);
        }

        // Called once AddPayment + Book_Ticket (Ticketing_Type "1") succeed for a
        // booking that started as a Block_Ticket hold — replaces the hold's own
        // Status_Id/PNR/RecordLocator with the real, ticketed ones. LocalStatus stays
        // Active: the hold-vs-ticketed distinction lives entirely in StatusId, same as
        // CancelTripBookingCommandHandler already branches on it.
        public void MarkTicketed(string statusId, string? airlinePnr, string? crsPnr, string? recordLocator)
        {
            StatusId = statusId;
            AirlinePnr = airlinePnr;
            CrsPnr = crsPnr;
            RecordLocator = recordLocator;
        }

        public void MarkCancelled(int cancellationType, string cancelCode)
        {
            LocalStatus = StatusCancelled;
            CancelledAt = DateTime.UtcNow;
            CancellationType = cancellationType;
            CancelCode = cancelCode;
        }

        // Cancels one leg on its own (e.g. the return leg of a roundtrip) instead of
        // the whole booking. Only flips the booking's own LocalStatus to Cancelled once
        // every leg has been cancelled this way — a booking with a still-active leg
        // stays Active so "My Trips" keeps showing/allowing action on what's left.
        public void MarkLegCancelled(int legIndex, int cancellationType, string cancelCode)
        {
            var leg = _legs.FirstOrDefault(l => l.LegIndex == legIndex);
            if (leg == null)
            {
                throw new InvalidOperationException($"Leg {legIndex} not found on booking {Id}.");
            }

            leg.MarkCancelled(cancellationType, cancelCode);

            if (_legs.All(l => l.IsCancelled))
            {
                MarkCancelled(cancellationType, cancelCode);
            }
        }

        public void MarkReleased()
        {
            LocalStatus = StatusReleased;
            CancelledAt = DateTime.UtcNow;
        }
    }
}
