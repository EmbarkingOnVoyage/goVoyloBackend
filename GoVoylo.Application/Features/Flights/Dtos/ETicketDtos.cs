namespace GoVoylo.Application.Features.Flights.Dtos
{
    // Everything printed on the e-ticket PDF emailed after ticketing (Figma
    // "Ticket Design"). Optional fields are left blank when the supplier didn't
    // return them.
    public record ETicketDocumentDto(
        string BookingRefNo,
        // "Domestic · One Way", "International · Round Trip", "Multi-city", ...
        string TripTitle,
        DateTime BookedOnUtc,
        IReadOnlyList<ETicketTripDto> Trips,
        IReadOnlyList<ETicketPassengerDto> Passengers,
        IReadOnlyList<ETicketBaggageRowDto> Baggage,
        ETicketPaymentDto Payment,
        string? ContactEmail,
        string? ContactPhone);

    // One trip of the booking (Departure / Return / Flight N) — one or more
    // connecting flights.
    public record ETicketTripDto(
        string Label,
        string? Pnr,
        string? CabinClass,
        string? Baggage,
        IReadOnlyList<ETicketSegmentDto> Segments);

    public record ETicketSegmentDto(
        string AirlineCode,
        string AirlineName,
        string FlightNumber,
        ETicketAirportDto From,
        ETicketAirportDto To,
        // Local airport times; Arrival is null when only the travel date is known.
        DateTime Departure,
        DateTime? Arrival,
        int DurationMinutes,
        int Stops);

    public record ETicketAirportDto(string Code, string? City, string? Name, string? Terminal);

    public record ETicketPassengerDto(string Name, string PaxType, IReadOnlyList<ETicketPassengerRouteDto> Routes);

    // Route: "DEL-BOM".
    public record ETicketPassengerRouteDto(
        string Route,
        string? CabinClass,
        string? Seat,
        string? Meal,
        string? ExtraBaggage,
        string? TicketNumber);

    public record ETicketBaggageRowDto(string PaxType, string Sector, string CheckIn, string Cabin);

    // Total is what the customer paid; the split is the supplier's, with any
    // difference shown as add-ons.
    public record ETicketPaymentDto(
        decimal? BaseFare,
        decimal? TaxesAndFees,
        decimal? AddOns,
        decimal Total,
        string CurrencyCode);
}
