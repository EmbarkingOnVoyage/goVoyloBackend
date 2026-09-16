using System.Text.Json.Serialization;

namespace GoVoylo.Infrastructure.ExternalServices.Flyshop
{
    // Field names and casing below are taken verbatim from Flyshop's "Client 2.6 Air"
    // Postman collection (Air_Search / Air_Reprice), including its inconsistent mix of
    // Snake_Case and PascalCase across nesting levels — that inconsistency is real, not
    // a typo, confirmed against the collection's own example request/response bodies.

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

    public class ResponseHeaderWire
    {
        [JsonPropertyName("Error_Code")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("Error_Desc")]
        public string? ErrorDesc { get; set; }

        [JsonPropertyName("Error_InnerException")]
        public string? ErrorInnerException { get; set; }

        [JsonPropertyName("Request_Id")]
        public string? RequestId { get; set; }

        [JsonPropertyName("Status_Id")]
        public string? StatusId { get; set; }
    }

    public class SegmentWire
    {
        [JsonPropertyName("Segment_Id")]
        public int SegmentId { get; set; }

        [JsonPropertyName("Origin")]
        public string Origin { get; set; } = string.Empty;

        [JsonPropertyName("Destination")]
        public string Destination { get; set; } = string.Empty;

        [JsonPropertyName("Airline_Code")]
        public string AirlineCode { get; set; } = string.Empty;

        [JsonPropertyName("Airline_Name")]
        public string AirlineName { get; set; } = string.Empty;

        [JsonPropertyName("Flight_Number")]
        public string FlightNumber { get; set; } = string.Empty;

        [JsonPropertyName("Departure_DateTime")]
        public string DepartureDateTime { get; set; } = string.Empty;

        [JsonPropertyName("Arrival_DateTime")]
        public string ArrivalDateTime { get; set; } = string.Empty;

        [JsonPropertyName("Duration")]
        public string Duration { get; set; } = string.Empty;
    }

    public class AirportTaxWire
    {
        [JsonPropertyName("Tax_Code")]
        public string? TaxCode { get; set; }

        [JsonPropertyName("Tax_Desc")]
        public string? TaxDesc { get; set; }

        [JsonPropertyName("Tax_Amount")]
        public decimal TaxAmount { get; set; }
    }

    public class FareClassWire
    {
        [JsonPropertyName("Segment_Id")]
        public int SegmentId { get; set; }

        [JsonPropertyName("Class_Code")]
        public string? ClassCode { get; set; }

        [JsonPropertyName("Class_Desc")]
        public string? ClassDesc { get; set; }

        [JsonPropertyName("FareBasis")]
        public string? FareBasis { get; set; }

        [JsonPropertyName("Privileges")]
        public List<string>? Privileges { get; set; }
    }

    public class FreeBaggageWire
    {
        [JsonPropertyName("Check_In_Baggage")]
        public string? CheckInBaggage { get; set; }

        [JsonPropertyName("Hand_Baggage")]
        public string? HandBaggage { get; set; }
    }

    public class FareDetailWire
    {
        // Documented as a string enum (0-ADT/1-CHD/2-INF) but the collection's own
        // sample response emits it as a JSON number.
        [JsonPropertyName("PAX_Type")]
        public int PaxType { get; set; }

        [JsonPropertyName("FareClasses")]
        public List<FareClassWire> FareClasses { get; set; } = new();

        [JsonPropertyName("Basic_Amount")]
        public decimal BasicAmount { get; set; }

        [JsonPropertyName("YQ_Amount")]
        public decimal YqAmount { get; set; }

        [JsonPropertyName("AirportTax_Amount")]
        public decimal AirportTaxAmount { get; set; }

        [JsonPropertyName("AirportTaxes")]
        public List<AirportTaxWire> AirportTaxes { get; set; } = new();

        [JsonPropertyName("Service_Fee_Amount")]
        public decimal ServiceFeeAmount { get; set; }

        [JsonPropertyName("Trade_Markup_Amount")]
        public decimal TradeMarkupAmount { get; set; }

        [JsonPropertyName("Promo_Discount")]
        public decimal PromoDiscount { get; set; }

        [JsonPropertyName("GST")]
        public decimal Gst { get; set; }

        [JsonPropertyName("TDS")]
        public decimal Tds { get; set; }

        [JsonPropertyName("Gross_Commission")]
        public decimal GrossCommission { get; set; }

        [JsonPropertyName("Net_Commission")]
        public decimal NetCommission { get; set; }

        [JsonPropertyName("Currency_Code")]
        public string CurrencyCode { get; set; } = "INR";

        [JsonPropertyName("Total_Amount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("Free_Baggage")]
        public FreeBaggageWire? FreeBaggage { get; set; }
    }

    public class FareWire
    {
        [JsonPropertyName("Fare_Id")]
        public string? FareId { get; set; }

        [JsonPropertyName("Fare_Key")]
        public string? FareKey { get; set; }

        // Documented as a string but the sample response emits a JSON boolean.
        [JsonPropertyName("Refundable")]
        public bool Refundable { get; set; }

        [JsonPropertyName("FareDetails")]
        public List<FareDetailWire> FareDetails { get; set; } = new();

        [JsonPropertyName("Food_onboard")]
        public string? FoodOnboard { get; set; }

        [JsonPropertyName("Seats_Available")]
        public string? SeatsAvailable { get; set; }

        [JsonPropertyName("LastFewSeats")]
        public string? LastFewSeats { get; set; }

        [JsonPropertyName("Warning")]
        public string? Warning { get; set; }

        [JsonPropertyName("PromptMessage")]
        public string? PromptMessage { get; set; }

        [JsonPropertyName("GSTMandatory")]
        public bool GstMandatory { get; set; }

        [JsonPropertyName("FareType")]
        public int FareType { get; set; }

        [JsonPropertyName("ProductClass")]
        public string? ProductClass { get; set; }
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

        [JsonPropertyName("TravelDate")]
        public string? TravelDate { get; set; }

        [JsonPropertyName("Segments")]
        public List<SegmentWire> Segments { get; set; } = new();

        [JsonPropertyName("Fares")]
        public List<FareWire> Fares { get; set; } = new();

        [JsonPropertyName("Airline_Code")]
        public string? AirlineCode { get; set; }

        [JsonPropertyName("Cached")]
        public bool Cached { get; set; }

        [JsonPropertyName("IsLCC")]
        public bool IsLcc { get; set; }

        [JsonPropertyName("Block_Ticket_Allowed")]
        public bool BlockTicketAllowed { get; set; }

        [JsonPropertyName("GST_Entry_Allowed")]
        public bool GstEntryAllowed { get; set; }

        [JsonPropertyName("Repriced")]
        public bool Repriced { get; set; }

        [JsonPropertyName("HasMoreClass")]
        public bool HasMoreClass { get; set; }

        [JsonPropertyName("InventoryType")]
        public int InventoryType { get; set; }

        [JsonPropertyName("IsFareChange")]
        public bool IsFareChange { get; set; }

        [JsonPropertyName("PromoCode")]
        public string? PromoCode { get; set; }
    }

    public class TripDetailWire
    {
        [JsonPropertyName("Trip_Id")]
        public int? TripId { get; set; }

        [JsonPropertyName("Flights")]
        public List<FlightWire> Flights { get; set; } = new();
    }

    public class AirSearchResponseWire
    {
        [JsonPropertyName("Response_Header")]
        public ResponseHeaderWire? ResponseHeader { get; set; }

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

        [JsonPropertyName("Customer_Mobile")]
        public string CustomerMobile { get; set; } = string.Empty;

        [JsonPropertyName("GST_Input")]
        public bool GstInput { get; set; }

        [JsonPropertyName("SinglePricing")]
        public bool SinglePricing { get; set; } = true;
    }

    public class AirLowFareRequestWire
    {
        [JsonPropertyName("Auth_Header")]
        public AuthHeaderWire AuthHeader { get; set; } = new();

        [JsonPropertyName("Origin")]
        public string Origin { get; set; } = string.Empty;

        [JsonPropertyName("Destination")]
        public string Destination { get; set; } = string.Empty;

        // Documented as MM (zero-padded, e.g. "05" for May).
        [JsonPropertyName("Month")]
        public string Month { get; set; } = string.Empty;

        // The doc's own example sends this as a JSON number (2022), not a string.
        [JsonPropertyName("Year")]
        public int Year { get; set; }
    }

    public class LowFareWire
    {
        [JsonPropertyName("AirlineCode")]
        public string? AirlineCode { get; set; }

        [JsonPropertyName("AirlinesName")]
        public string? AirlinesName { get; set; }

        [JsonPropertyName("Amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("TravelDate")]
        public string? TravelDate { get; set; }
    }

    public class AirLowFareResponseWire
    {
        [JsonPropertyName("Response_Header")]
        public ResponseHeaderWire? ResponseHeader { get; set; }

        [JsonPropertyName("LowFares")]
        public List<LowFareWire> LowFares { get; set; } = new();
    }

    public class AirRepriceResponseItemWire
    {
        [JsonPropertyName("Flight")]
        public FlightWire? Flight { get; set; }
    }

    // The doc's own Air_Reprice response table spells this "ResponseHeader" (no
    // underscore) while Air_Search's confirmed sample uses "Response_Header" — the
    // collection is inconsistent between sections; we key off Air_Search's confirmed
    // casing here since Response_Header is the only one seen in an actual JSON sample.
    public class AirRepriceResponseWire
    {
        [JsonPropertyName("Response_Header")]
        public ResponseHeaderWire? ResponseHeader { get; set; }

        [JsonPropertyName("AirRepriceResponses")]
        public List<AirRepriceResponseItemWire> AirRepriceResponses { get; set; } = new();

        [JsonPropertyName("FrequentFlyerAccepted")]
        public bool FrequentFlyerAccepted { get; set; }
    }
}
