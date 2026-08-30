namespace GoVoylo.Infrastructure.Jobs
{
    // Starter dataset standing in for a real airport master file (GV-FLT-BE-002).
    // AirportImportService upserts on IATA code, so swapping this for a real CSV-driven
    // import later is a drop-in replacement, not a rewrite.
    public static class AirportSeedData
    {
        public static readonly IReadOnlyList<(string Iata, string Name, string City, string Country, bool Popular)> Airports = new[]
        {
            ("BOM", "Chhatrapati Shivaji Maharaj International Airport", "Mumbai", "India", true),
            ("DEL", "Indira Gandhi International Airport", "Delhi", "India", true),
            ("BLR", "Kempegowda International Airport", "Bengaluru", "India", true),
            ("MAA", "Chennai International Airport", "Chennai", "India", true),
            ("CCU", "Netaji Subhas Chandra Bose International Airport", "Kolkata", "India", false),
            ("HYD", "Rajiv Gandhi International Airport", "Hyderabad", "India", true),
            ("GOI", "Goa International Airport", "Goa", "India", false),
            ("COK", "Cochin International Airport", "Kochi", "India", false),
            ("AMD", "Sardar Vallabhbhai Patel International Airport", "Ahmedabad", "India", false),
            ("PNQ", "Pune Airport", "Pune", "India", false),
            ("JAI", "Jaipur International Airport", "Jaipur", "India", false),
            ("LKO", "Chaudhary Charan Singh International Airport", "Lucknow", "India", false),
            ("DXB", "Dubai International Airport", "Dubai", "United Arab Emirates", true),
            ("AUH", "Zayed International Airport", "Abu Dhabi", "United Arab Emirates", false),
            ("SIN", "Singapore Changi Airport", "Singapore", "Singapore", true),
            ("BKK", "Suvarnabhumi Airport", "Bangkok", "Thailand", true),
            ("KUL", "Kuala Lumpur International Airport", "Kuala Lumpur", "Malaysia", false),
            ("HKG", "Hong Kong International Airport", "Hong Kong", "Hong Kong", false),
            ("ICN", "Incheon International Airport", "Seoul", "South Korea", false),
            ("NRT", "Narita International Airport", "Tokyo", "Japan", false),
            ("DOH", "Hamad International Airport", "Doha", "Qatar", false),
            ("LHR", "London Heathrow Airport", "London", "United Kingdom", true),
            ("CDG", "Charles de Gaulle Airport", "Paris", "France", false),
            ("FRA", "Frankfurt Airport", "Frankfurt", "Germany", false),
            ("AMS", "Amsterdam Airport Schiphol", "Amsterdam", "Netherlands", false),
            ("IST", "Istanbul Airport", "Istanbul", "Turkey", false),
            ("JFK", "John F. Kennedy International Airport", "New York", "United States", true),
            ("ORD", "O'Hare International Airport", "Chicago", "United States", false),
            ("LAX", "Los Angeles International Airport", "Los Angeles", "United States", false),
            ("YYZ", "Toronto Pearson International Airport", "Toronto", "Canada", false),
            ("SYD", "Sydney Kingsford Smith Airport", "Sydney", "Australia", false),
            ("MEL", "Melbourne Airport", "Melbourne", "Australia", false),
        };
    }
}
