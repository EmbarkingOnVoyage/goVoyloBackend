namespace GoVoylo.Application.Features.Flights.Dtos
{
    // Everything printed on the e-ticket PDF emailed after ticketing (Figma
    // "Ticket Design"). Optional fields are left blank when the supplier didn't
    // return them.
    public record ETicketDocumentDto(
        string BookingRefNo,
        // GoVoylo's own reference for the booking (from its internal id).
        string RefId,
        // "Domestic · One Way", "International · Round Trip", "Multi-city", ...
        string TripTitle,
        DateTime BookedOnUtc,
        IReadOnlyList<ETicketTripDto> Trips,
        IReadOnlyList<ETicketPassengerDto> Passengers,
        IReadOnlyList<ETicketBaggageRowDto> Baggage,
        ETicketPaymentDto Payment,
        string? ContactEmail,
        string? ContactPhone,
        bool IsMultiCity = false,
        // "2 Adults + 1 Child" — shown next to Base Fare when there's more than one.
        string? PassengerSummary = null);

    // One trip of the booking (Departure / Return / Flight N) — one or more
    // connecting flights.
    public record ETicketTripDto(
        string Label,
        string? Pnr,
        string? CabinClass,
        string? Baggage,
        IReadOnlyList<ETicketSegmentDto> Segments,
        // "Regular Fare", "SME Fare", ... — null when the booking didn't record it.
        string? FareType = null);

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
        string? TicketNumber,
        // The airline PNR for this route — the barcode's fallback when there's no
        // e-ticket number.
        string? Pnr = null);

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
