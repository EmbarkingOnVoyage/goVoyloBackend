namespace GoVoylo.Domain.Entities
{
    // One flown segment-group of a TripBooking — one row per leg (outbound/return/
    // multi-city). FlightId is Flyshop's own per-flight identifier (distinct from
    // Flight_Key, which only lives for the duration of a search session) — it's
    // required to cancel or release this specific leg later via Air_TicketCancellation
    // / Air_ReleasePNR.
    public class TripBookingLeg
    {
        public Guid Id { get; private set; }
        public Guid TripBookingId { get; private set; }
        public int LegIndex { get; private set; }
        public string Origin { get; private set; } = null!;
        public string Destination { get; private set; } = null!;
        public DateTime TravelDate { get; private set; }
        public string AirlineCode { get; private set; } = null!;
        public string AirlineName { get; private set; } = null!;
        public string FlightNumber { get; private set; } = null!;
        public string FlightId { get; private set; } = null!;

        // Set when this leg is cancelled on its own via Air_TicketCancellation
        // (e.g. cancelling only the return leg of a roundtrip) rather than the whole
        // booking. TripBooking.LocalStatus only flips to Cancelled once every leg here
        // is cancelled — see TripBooking.MarkLegCancelled.
        public bool IsCancelled { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public int? CancellationType { get; private set; }
        public string? CancelCode { get; private set; }

        public TripBookingLeg(
            Guid tripBookingId,
            int legIndex,
            string origin,
            string destination,
            DateTime travelDate,
            string airlineCode,
            string airlineName,
            string flightNumber,
            string flightId)
        {
            Id = Guid.NewGuid();
            TripBookingId = tripBookingId;
            LegIndex = legIndex;
            Origin = origin;
            Destination = destination;
            TravelDate = travelDate;
            AirlineCode = airlineCode;
            AirlineName = airlineName;
            FlightNumber = flightNumber;
            FlightId = flightId;
        }

        // Required by EF Core
        private TripBookingLeg()
        {
        }

        public void MarkCancelled(int cancellationType, string cancelCode)
        {
            IsCancelled = true;
            CancelledAt = DateTime.UtcNow;
            CancellationType = cancellationType;
            CancelCode = cancelCode;
        }
    }
}
