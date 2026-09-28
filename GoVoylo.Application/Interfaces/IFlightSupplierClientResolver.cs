namespace GoVoylo.Application.Interfaces
{
    // Picks the right IFlightSupplierClient for a booking/offer that's already tied
    // to one supplier (see TripBooking.SupplierCode / FlightOfferSession.SupplierCode).
    // Search is the one place that talks to every registered supplier at once (see
    // SearchFlightsQueryHandler, which injects IEnumerable<IFlightSupplierClient>
    // directly instead) — everywhere else operates on a single already-chosen supplier.
    public interface IFlightSupplierClientResolver
    {
        IFlightSupplierClient Resolve(string supplierCode);
    }
}
