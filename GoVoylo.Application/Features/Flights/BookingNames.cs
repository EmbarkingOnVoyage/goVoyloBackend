namespace GoVoylo.Application.Features.Flights
{
    // Both suppliers require a last name (Tripjack travellerInfo.lN, Flyshop
    // Last_Name). A traveller with a single name is saved without one, and booked
    // with the first name repeated as surname — the usual airline practice.
    public static class BookingNames
    {
        public static string Surname(string firstName, string? lastName) =>
            string.IsNullOrWhiteSpace(lastName) ? firstName.Trim() : lastName.Trim();
    }
}
