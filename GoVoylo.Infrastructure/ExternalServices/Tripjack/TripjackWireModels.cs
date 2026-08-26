using System.Text.Json.Serialization;

namespace GoVoylo.Infrastructure.ExternalServices.Tripjack
{
    public class AuthHeaderWire
    {
        [JsonPropertyName("UserId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("Password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("IP_Address")]
        public string IpAddress { get; set; } = string.Empty;

        [JsonPropertyName("Request_Id")]
        public string RequestId { get; set; } = string.Empty;

        [JsonPropertyName("IMEI_Number")]
        public string ImeiNumber { get; set; } = string.Empty;
    }

    public class TripInfoWire
    {
        [JsonPropertyName("Origin")]
        public string Origin { get; set; } = string.Empty;

        [JsonPropertyName("Destination")]
        public string Destination { get; set; } = string.Empty;

        [JsonPropertyName("TravelDate")]
        public string TravelDate { get; set; } = string.Empty;

        [JsonPropertyName("Trip_Id")]
        public int TripId { get; set; }
    }

    public class FilteredAirlineWire
    {
        [JsonPropertyName("Airline_Code")]
        public string AirlineCode { get; set; } = string.Empty;
    }

    public class AirSearchRequestWire
    {
        [JsonPropertyName("Auth_Header")]
        public AuthHeaderWire AuthHeader { get; set; } = new();

        [JsonPropertyName("Travel_Type")]
        public int TravelType { get; set; }

        [JsonPropertyName("Booking_Type")]
        public int BookingType { get; set; }

        [JsonPropertyName("TripInfo")]
        public List<TripInfoWire> TripInfo { get; set; } = new();

        [JsonPropertyName("Adult_Count")]
        public string AdultCount { get; set; } = "1";

        [JsonPropertyName("Child_Count")]
        public string ChildCount { get; set; } = "0";

        [JsonPropertyName("Infant_Count")]
        public string InfantCount { get; set; } = "0";

        [JsonPropertyName("Class_Of_Travel")]
        public string ClassOfTravel { get; set; } = "0";

        [JsonPropertyName("InventoryType")]
        public int InventoryType { get; set; }

        [JsonPropertyName("Source_Type")]
        public int SourceType { get; set; }

        [JsonPropertyName("Filtered_Airline")]
        public List<FilteredAirlineWire> FilteredAirline { get; set; } = new();
    }

    public class SegmentWire
    {
        [JsonPropertyName("Origin")]
        public string Origin { get; set; } = string.Empty;

        [JsonPropertyName("Destination")]
        public string Destination { get; set; } = string.Empty;

        [JsonPropertyName("AirlineCode")]
        public string AirlineCode { get; set; } = string.Empty;

        [JsonPropertyName("AirlineName")]
        public string AirlineName { get; set; } = string.Empty;

        [JsonPropertyName("FlightNumber")]
        public string FlightNumber { get; set; } = string.Empty;

        [JsonPropertyName("DepartureDateTime")]
        public string DepartureDateTime { get; set; } = string.Empty;

        [JsonPropertyName("ArrivalDateTime")]
        public string ArrivalDateTime { get; set; } = string.Empty;

        [JsonPropertyName("Duration")]
        public string Duration { get; set; } = string.Empty;
    }

    public class FareDetailWire
    {
        [JsonPropertyName("PAXType")]
        public string? PaxType { get; set; }

        [JsonPropertyName("TotalAmount")]
        public decimal? TotalAmount { get; set; }

        [JsonPropertyName("CurrencyCode")]
        public string? CurrencyCode { get; set; }
    }

    public class FareWire
    {
        [JsonPropertyName("FareId")]
        public string? FareId { get; set; }

        [JsonPropertyName("FareKey")]
        public string? FareKey { get; set; }

        [JsonPropertyName("Refundable")]
        public string? Refundable { get; set; }

        [JsonPropertyName("SeatsAvailable")]
        public string? SeatsAvailable { get; set; }

        [JsonPropertyName("FareDetails")]
        public List<FareDetailWire> FareDetails { get; set; } = new();
    }

    public class FlightWire
    {
        [JsonPropertyName("Flight_Id")]
        public string? FlightId { get; set; }

        [JsonPropertyName("Flight_Key")]
        public string FlightKey { get; set; } = string.Empty;

        [JsonPropertyName("Origin")]
        public string Origin { get; set; } = string.Empty;

        [JsonPropertyName("Destination")]
        public string Destination { get; set; } = string.Empty;

        [JsonPropertyName("Segments")]
        public List<SegmentWire> Segments { get; set; } = new();

        [JsonPropertyName("Fares")]
        public List<FareWire> Fares { get; set; } = new();

        [JsonPropertyName("Airline_Code")]
        public string? AirlineCode { get; set; }

        [JsonPropertyName("IsLCC")]
        public bool IsLcc { get; set; }

        [JsonPropertyName("Repriced")]
        public bool Repriced { get; set; }

        [JsonPropertyName("IsFareChange")]
        public bool IsFareChange { get; set; }
    }

    public class TripDetailWire
    {
        [JsonPropertyName("Trip_Id")]
        public string? TripId { get; set; }

        [JsonPropertyName("Flights")]
        public List<FlightWire> Flights { get; set; } = new();
    }

    public class AirSearchResponseWire
    {
        [JsonPropertyName("Search_Key")]
        public string SearchKey { get; set; } = string.Empty;

        [JsonPropertyName("TripDetails")]
        public List<TripDetailWire> TripDetails { get; set; } = new();
    }

    public class AirRepriceRequestItemWire
    {
        [JsonPropertyName("Flight_Key")]
        public string FlightKey { get; set; } = string.Empty;

        [JsonPropertyName("Fare_Id")]
        public string FareId { get; set; } = string.Empty;
    }

    public class AirRepriceRequestWire
    {
        [JsonPropertyName("Auth_Header")]
        public AuthHeaderWire AuthHeader { get; set; } = new();

        [JsonPropertyName("Search_Key")]
        public string SearchKey { get; set; } = string.Empty;

        [JsonPropertyName("AirRepriceRequests")]
        public List<AirRepriceRequestItemWire> AirRepriceRequests { get; set; } = new();

        [JsonPropertyName("GST_Input")]
        public bool GstInput { get; set; }

        [JsonPropertyName("SinglePricing")]
        public bool SinglePricing { get; set; } = true;
    }

    // NOTE: Tripjack's doc only describes the Reprice *request* schema in full; the
    // response is only described as "returns the flight object, with Repriced as true".
    // This mirrors the Air_Search flight shape as the best available inference — confirm
    // against a real response once live credentials are available and adjust if the
    // envelope key differs.
    public class AirRepriceResponseWire
    {
        [JsonPropertyName("AirRepriceResponses")]
        public List<FlightWire> AirRepriceResponses { get; set; } = new();
    }
}
