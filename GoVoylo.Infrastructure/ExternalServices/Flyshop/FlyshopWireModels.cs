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

    // Field names below are taken verbatim from the Postman collection's own
    // *example response bodies* for Air_GetSSR/Air_GetSeatMap, not from its field
    // description tables — the two disagree (e.g. the table says "SegmentWise" and
    // "TotalAmount", the real sample JSON says "Segment_Wise" and "Total_Amount"),
    // and the samples are what the sandbox actually returns.

    public class AirSsrRequestItemWire
    {
        [JsonPropertyName("Flight_Key")]
        public string FlightKey { get; set; } = string.Empty;
    }

    public class AirSsrRequestWire
    {
        [JsonPropertyName("Auth_Header")]
        public AuthHeaderWire AuthHeader { get; set; } = new();

        [JsonPropertyName("Search_Key")]
        public string SearchKey { get; set; } = string.Empty;

        [JsonPropertyName("AirSSRRequestDetails")]
        public List<AirSsrRequestItemWire> AirSsrRequestDetails { get; set; } = new();
    }

    // Shared by Air_GetSSR's SSRDetails and Air_GetSeatMap's Seat_Details — both
    // endpoints' real sample responses emit an identical SSR-detail shape.
    public class SsrDetailWire
    {
        [JsonPropertyName("ApplicablePaxTypes")]
        public List<int> ApplicablePaxTypes { get; set; } = new();

        [JsonPropertyName("Currency_Code")]
        public string? CurrencyCode { get; set; }

        [JsonPropertyName("Flight_ID")]
        public string? FlightId { get; set; }

        [JsonPropertyName("Leg_Index")]
        public int LegIndex { get; set; }

        [JsonPropertyName("SSR_Code")]
        public string? SsrCode { get; set; }

        [JsonPropertyName("SSR_Key")]
        public string? SsrKey { get; set; }

        [JsonPropertyName("SSR_Status")]
        public int SsrStatus { get; set; }

        [JsonPropertyName("SSR_Type")]
        public int SsrType { get; set; }

        [JsonPropertyName("SSR_TypeDesc")]
        public string? SsrTypeDesc { get; set; }

        [JsonPropertyName("SSR_TypeName")]
        public string? SsrTypeName { get; set; }

        [JsonPropertyName("Segment_Id")]
        public int SegmentId { get; set; }

        [JsonPropertyName("Segment_Wise")]
        public bool SegmentWise { get; set; }

        [JsonPropertyName("Total_Amount")]
        public decimal TotalAmount { get; set; }
    }

    public class SsrFlightDetailWire
    {
        [JsonPropertyName("SSRDetails")]
        public List<SsrDetailWire> SsrDetails { get; set; } = new();
    }

    public class AirSsrResponseWire
    {
        [JsonPropertyName("Response_Header")]
        public ResponseHeaderWire? ResponseHeader { get; set; }

        [JsonPropertyName("SSRFlightDetails")]
        public List<SsrFlightDetailWire> SsrFlightDetails { get; set; } = new();
    }

    // Sample request only ever populates Pax_Id/Pax_type/Title/First_Name/Last_Name/
    // Gender and sends every other field null — confirms Air_GetSeatMap works with
    // minimal traveler data, so we never need passport/DOB details just to price seats.
    public class PaxDetailWire
    {
        [JsonPropertyName("Pax_Id")]
        public int PaxId { get; set; }

        [JsonPropertyName("Pax_type")]
        public int PaxType { get; set; }

        [JsonPropertyName("Title")]
        public string? Title { get; set; }

        [JsonPropertyName("First_Name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("Last_Name")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("Gender")]
        public int Gender { get; set; }
    }

    public class AirSeatMapRequestWire
    {
        [JsonPropertyName("Auth_Header")]
        public AuthHeaderWire AuthHeader { get; set; } = new();

        [JsonPropertyName("Search_Key")]
        public string SearchKey { get; set; } = string.Empty;

        [JsonPropertyName("Flight_Keys")]
        public List<string> FlightKeys { get; set; } = new();

        [JsonPropertyName("PAX_Details")]
        public List<PaxDetailWire> PaxDetails { get; set; } = new();
    }

    public class SeatRowWire
    {
        [JsonPropertyName("Seat_Details")]
        public List<SsrDetailWire> SeatDetails { get; set; } = new();
    }

    public class SeatSegmentWire
    {
        [JsonPropertyName("Leg_Index")]
        public int LegIndex { get; set; }

        [JsonPropertyName("Seat_Row")]
        public List<SeatRowWire> SeatRow { get; set; } = new();
    }

    public class AirSeatMapWire
    {
        [JsonPropertyName("Flight_Id")]
        public string? FlightId { get; set; }

        [JsonPropertyName("Seat_Segments")]
        public List<SeatSegmentWire> SeatSegments { get; set; } = new();
    }

    public class AirSeatMapResponseWire
    {
        [JsonPropertyName("Response_Header")]
        public ResponseHeaderWire? ResponseHeader { get; set; }

        [JsonPropertyName("AirSeatMaps")]
        public List<AirSeatMapWire> AirSeatMaps { get; set; } = new();
    }

    // Field names again taken from the Postman collection's real example bodies —
    // note Air_Ticketing's request/response use "Booking_RefNo" (underscore) while
    // the field-description table for both endpoints says "BookingRefNo" (no
    // underscore); the sample JSON wins, same rationale as the SSR/SeatMap models.
    public class TempBookingPaxDetailWire
    {
        [JsonPropertyName("Pax_Id")]
        public int PaxId { get; set; }

        [JsonPropertyName("Pax_type")]
        public int PaxType { get; set; }

        [JsonPropertyName("Title")]
        public string? Title { get; set; }

        [JsonPropertyName("First_Name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("Last_Name")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("Gender")]
        public int Gender { get; set; }

        // Left null like the collection's own sample — full passport/DOB capture for
        // international/Book_Ticket itineraries is future work (see
        // FLIGHT_ANCILLARIES_SCOPE.MD-style follow-up), not needed for a domestic
        // Block_Ticket hold.
        [JsonPropertyName("Age")]
        public string? Age { get; set; }

        [JsonPropertyName("DOB")]
        public string? Dob { get; set; }

        [JsonPropertyName("Passport_Number")]
        public string? PassportNumber { get; set; }

        [JsonPropertyName("Passport_Issuing_Country")]
        public string? PassportIssuingCountry { get; set; }

        [JsonPropertyName("Passport_Expiry")]
        public string? PassportExpiry { get; set; }

        [JsonPropertyName("Nationality")]
        public string? Nationality { get; set; }

        [JsonPropertyName("Pancard_Number")]
        public string? PancardNumber { get; set; }

        [JsonPropertyName("FrequentFlyerDetails")]
        public object? FrequentFlyerDetails { get; set; }
    }

    public class BookingSsrDetailWire
    {
        [JsonPropertyName("Pax_Id")]
        public int PaxId { get; set; }

        [JsonPropertyName("SSR_Key")]
        public string SsrKey { get; set; } = string.Empty;
    }

    public class BookingFlightDetailWire
    {
        [JsonPropertyName("Search_Key")]
        public string SearchKey { get; set; } = string.Empty;

        [JsonPropertyName("Flight_Key")]
        public string FlightKey { get; set; } = string.Empty;

        [JsonPropertyName("BookingSSRDetails")]
        public List<BookingSsrDetailWire> BookingSsrDetails { get; set; } = new();
    }

    public class AirTempBookingRequestWire
    {
        [JsonPropertyName("Auth_Header")]
        public AuthHeaderWire AuthHeader { get; set; } = new();

        [JsonPropertyName("Customer_Mobile")]
        public string CustomerMobile { get; set; } = string.Empty;

        [JsonPropertyName("Passenger_Mobile")]
        public string PassengerMobile { get; set; } = string.Empty;

        [JsonPropertyName("WhatsAPP_Mobile")]
        public string? WhatsappMobile { get; set; }

        [JsonPropertyName("Passenger_Email")]
        public string PassengerEmail { get; set; } = string.Empty;

        [JsonPropertyName("PAX_Details")]
        public List<TempBookingPaxDetailWire> PaxDetails { get; set; } = new();

        [JsonPropertyName("GST")]
        public bool Gst { get; set; }

        [JsonPropertyName("GST_Number")]
        public string GstNumber { get; set; } = string.Empty;

        [JsonPropertyName("GST_HolderName")]
        public string GstHolderName { get; set; } = string.Empty;

        [JsonPropertyName("GST_Address")]
        public string GstAddress { get; set; } = string.Empty;

        [JsonPropertyName("BookingFlightDetails")]
        public List<BookingFlightDetailWire> BookingFlightDetails { get; set; } = new();
    }

    public class AirTempBookingResponseWire
    {
        [JsonPropertyName("Booking_RefNo")]
        public string? BookingRefNo { get; set; }

        [JsonPropertyName("Response_Header")]
        public ResponseHeaderWire? ResponseHeader { get; set; }
    }

    public class AirTicketingRequestWire
    {
        [JsonPropertyName("Auth_Header")]
        public AuthHeaderWire AuthHeader { get; set; } = new();

        [JsonPropertyName("Booking_RefNo")]
        public string BookingRefNo { get; set; } = string.Empty;

        // Deliberately hardcoded to "0" (Block_Ticket) at the call site — see
        // IFlightSupplierClient.CreateBlockTicketAsync's own doc comment for why
        // Book_Ticket (real purchase + Add_Payment wallet debit) isn't wired up.
        [JsonPropertyName("Ticketing_Type")]
        public string TicketingType { get; set; } = "0";
    }

    public class AirlinePnrWire
    {
        [JsonPropertyName("Airline_Code")]
        public string? AirlineCode { get; set; }

        [JsonPropertyName("Airline_PNR")]
        public string? AirlinePnr { get; set; }

        [JsonPropertyName("CRS_Code")]
        public string? CrsCode { get; set; }

        [JsonPropertyName("CRS_PNR")]
        public string? CrsPnr { get; set; }

        [JsonPropertyName("Record_Locator")]
        public string? RecordLocator { get; set; }

        [JsonPropertyName("Supplier_RefNo")]
        public string? SupplierRefNo { get; set; }
    }

    public class AirlinePnrDetailWire
    {
        [JsonPropertyName("Flight_Id")]
        public string? FlightId { get; set; }

        // 11-Success, 22-Failed, 33-Block, per the docs' own note.
        [JsonPropertyName("Status_Id")]
        public string? StatusId { get; set; }

        [JsonPropertyName("Failure_Remark")]
        public string? FailureRemark { get; set; }

        [JsonPropertyName("Hold_Validity")]
        public string? HoldValidity { get; set; }

        [JsonPropertyName("AirlinePNRs")]
        public List<AirlinePnrWire> AirlinePnrs { get; set; } = new();
    }

    public class AirTicketingResponseWire
    {
        [JsonPropertyName("Response_Header")]
        public ResponseHeaderWire? ResponseHeader { get; set; }

        [JsonPropertyName("Booking_RefNo")]
        public string? BookingRefNo { get; set; }

        [JsonPropertyName("AirlinePNRDetails")]
        public List<AirlinePnrDetailWire> AirlinePnrDetails { get; set; } = new();
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

    // FareRuleDesc is a full XHTML document string, not structured data — the
    // collection's own sample response confirms this (a whole <html><head>...
    // <style>...</style></head><body>... blob for a single sentence of real text).
    public class FareRuleWire
    {
        [JsonPropertyName("Segment_Id")]
        public string? SegmentId { get; set; }

        [JsonPropertyName("FareRuleName")]
        public string? FareRuleName { get; set; }

        [JsonPropertyName("FareRuleDesc")]
        public string? FareRuleDesc { get; set; }
    }

    public class AirFareRuleResponseWire
    {
        [JsonPropertyName("Response_Header")]
        public ResponseHeaderWire? ResponseHeader { get; set; }

        [JsonPropertyName("FareRules")]
        public List<FareRuleWire> FareRules { get; set; } = new();
    }

    public class AirTicketCancelDetailWire
    {
        [JsonPropertyName("FlightId")]
        public string FlightId { get; set; } = string.Empty;

        [JsonPropertyName("PassengerId")]
        public string PassengerId { get; set; } = string.Empty;

        [JsonPropertyName("SegmentId")]
        public string SegmentId { get; set; } = string.Empty;
    }

    // Real endpoint name is Air_TicketCancellation (the "10 - Air_Cancellation"
    // sidebar entry is just the collection's shorthand label for it).
    public class AirTicketCancellationRequestWire
    {
        [JsonPropertyName("Auth_Header")]
        public AuthHeaderWire AuthHeader { get; set; } = new();

        [JsonPropertyName("AirTicketCancelDetails")]
        public List<AirTicketCancelDetailWire> AirTicketCancelDetails { get; set; } = new();

        [JsonPropertyName("Airline_PNR")]
        public string AirlinePnr { get; set; } = string.Empty;

        [JsonPropertyName("RefNo")]
        public string RefNo { get; set; } = string.Empty;

        [JsonPropertyName("CancelCode")]
        public string CancelCode { get; set; } = string.Empty;

        [JsonPropertyName("ReqRemarks")]
        public string ReqRemarks { get; set; } = string.Empty;

        [JsonPropertyName("CancellationType")]
        public int CancellationType { get; set; }
    }

    public class AirTicketCancellationResponseWire
    {
        [JsonPropertyName("Response_Header")]
        public ResponseHeaderWire? ResponseHeader { get; set; }
    }
}
