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

    // Every Tripjack response carries this envelope. Error_Code "0000" is the only
    // documented success value — anything else (or a non-"SUCCESS" Error_Desc) is a
    // business-level failure even though the HTTP status itself comes back 200.
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

    public interface ITripjackEnvelope
    {
        ResponseHeaderWire? ResponseHeader { get; }
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

    public class FreeBaggageWire
    {
        [JsonPropertyName("Check_In_Baggage")]
        public string? CheckInBaggage { get; set; }

        [JsonPropertyName("Hand_Baggage")]
        public string? HandBaggage { get; set; }
    }

    // Tripjack's own field is misspelled "Applicablility" — kept verbatim since
    // [JsonPropertyName] must match the wire exactly.
    public class RescheduleChargeWire
    {
        [JsonPropertyName("Applicablility")]
        public int Applicability { get; set; }

        [JsonPropertyName("PassengerType")]
        public int PassengerType { get; set; }

        [JsonPropertyName("Value")]
        public string? Value { get; set; }

        [JsonPropertyName("ValueType")]
        public int ValueType { get; set; }

        [JsonPropertyName("DurationFrom")]
        public int DurationFrom { get; set; }

        [JsonPropertyName("DurationTo")]
        public int DurationTo { get; set; }

        [JsonPropertyName("DurationTypeFrom")]
        public int DurationTypeFrom { get; set; }

        [JsonPropertyName("DurationTypeTo")]
        public int DurationTypeTo { get; set; }

        [JsonPropertyName("OnlineServiceFee")]
        public decimal OnlineServiceFee { get; set; }

        [JsonPropertyName("OfflineServiceFee")]
        public decimal OfflineServiceFee { get; set; }

        [JsonPropertyName("Remarks")]
        public string? Remarks { get; set; }
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
    }

    public class FareDetailWire
    {
        [JsonPropertyName("PAX_Type")]
        public int PaxType { get; set; }

        [JsonPropertyName("Basic_Amount")]
        public decimal BasicAmount { get; set; }

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

        [JsonPropertyName("Total_Amount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("Currency_Code")]
        public string? CurrencyCode { get; set; }

        [JsonPropertyName("Free_Baggage")]
        public FreeBaggageWire? FreeBaggage { get; set; }

        [JsonPropertyName("RescheduleCharges")]
        public List<RescheduleChargeWire> RescheduleCharges { get; set; } = new();

        [JsonPropertyName("FareClasses")]
        public List<FareClassWire> FareClasses { get; set; } = new();
    }

    public class FareWire
    {
        [JsonPropertyName("Fare_Id")]
        public string? FareId { get; set; }

        [JsonPropertyName("Fare_Key")]
        public string? FareKey { get; set; }

        [JsonPropertyName("Refundable")]
        public bool Refundable { get; set; }

        [JsonPropertyName("Seats_Available")]
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

    public class AirSearchResponseWire : ITripjackEnvelope
    {
        [JsonPropertyName("Search_Key")]
        public string SearchKey { get; set; } = string.Empty;

        [JsonPropertyName("TripDetails")]
        public List<TripDetailWire> TripDetails { get; set; } = new();

        [JsonPropertyName("Response_Header")]
        public ResponseHeaderWire? ResponseHeader { get; set; }
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

    public class AirRepriceResponseItemWire
    {
        [JsonPropertyName("Flight")]
        public FlightWire Flight { get; set; } = new();
    }

    public class AirRepriceResponseWire : ITripjackEnvelope
    {
        [JsonPropertyName("AirRepriceResponses")]
        public List<AirRepriceResponseItemWire> AirRepriceResponses { get; set; } = new();

        [JsonPropertyName("Response_Header")]
        public ResponseHeaderWire? ResponseHeader { get; set; }
    }

    public class AirFareRuleRequestWire
    {
        [JsonPropertyName("Auth_Header")]
        public AuthHeaderWire AuthHeader { get; set; } = new();

        [JsonPropertyName("Search_Key")]
        public string SearchKey { get; set; } = string.Empty;

        [JsonPropertyName("Flight_Key")]
        public string FlightKey { get; set; } = string.Empty;

        [JsonPropertyName("Fare_Id")]
        public string FareId { get; set; } = string.Empty;
    }

    public class FareRuleWire
    {
        [JsonPropertyName("Segment_Id")]
        public string? SegmentId { get; set; }

        [JsonPropertyName("FareRuleName")]
        public string? FareRuleName { get; set; }

        [JsonPropertyName("FareRuleDesc")]
        public string? FareRuleDesc { get; set; }
    }

    public class AirFareRuleResponseWire : ITripjackEnvelope
    {
        [JsonPropertyName("FareRules")]
        public List<FareRuleWire> FareRules { get; set; } = new();

        [JsonPropertyName("Response_Header")]
        public ResponseHeaderWire? ResponseHeader { get; set; }
    }
}
