using System.Text.Json;
using FluentAssertions;
using GoVoylo.Infrastructure.ExternalServices.Tripjack;

namespace GoVoylo.Infrastructure.UnitTests.ExternalServices.Tripjack
{
    // Tripjack's real JSON uses underscored keys (Total_Amount, Fare_Id, Seats_Available,
    // Airline_Name, PAX_Type...) that do NOT match the plain PascalCase names used in
    // Tripjack's own prose documentation tables (TotalAmount, FareId...). These fixtures
    // are trimmed-down copies of the actual sample payloads from Tripjack's published
    // Postman collection (Client 2.6 Air), so a future edit that "corrects" a
    // [JsonPropertyName] back to the prose-doc spelling fails loudly here instead of
    // silently deserializing to null/0 against the real API.
    public class TripjackWireModelsTests
    {
        private const string AirSearchResponseJson = """
        {
            "Search_Key": "abc123",
            "Response_Header": { "Error_Code": "0000", "Error_Desc": "SUCCESS" },
            "TripDetails": [
                {
                    "Flights": [
                        {
                            "Flight_Id": "F1",
                            "Flight_Key": "FKEY1",
                            "Origin": "BOM",
                            "Destination": "MAA",
                            "Airline_Code": "SG",
                            "IsLCC": true,
                            "Repriced": false,
                            "Segments": [
                                {
                                    "Segment_Id": 0,
                                    "Origin": "BOM",
                                    "Destination": "MAA",
                                    "Airline_Code": "SG",
                                    "Airline_Name": "SpiceJet",
                                    "Flight_Number": "6287",
                                    "Departure_DateTime": "01/25/2022 00:50",
                                    "Arrival_DateTime": "01/25/2022 02:40",
                                    "Duration": "01:50"
                                }
                            ],
                            "Fares": [
                                {
                                    "Fare_Id": "FARE1",
                                    "Fare_Key": "FAREKEY1",
                                    "Refundable": true,
                                    "Seats_Available": "4",
                                    "FareDetails": [
                                        {
                                            "PAX_Type": 0,
                                            "Basic_Amount": 1924,
                                            "AirportTax_Amount": 563,
                                            "AirportTaxes": [
                                                { "Tax_Code": "YQ", "Tax_Desc": "YQ", "Tax_Amount": 100 }
                                            ],
                                            "Service_Fee_Amount": 0,
                                            "Trade_Markup_Amount": 0,
                                            "Promo_Discount": 0,
                                            "GST": 5,
                                            "TDS": 0,
                                            "Total_Amount": 2487,
                                            "Currency_Code": "INR",
                                            "Free_Baggage": { "Check_In_Baggage": "15 KG", "Hand_Baggage": null },
                                            "RescheduleCharges": [
                                                {
                                                    "Applicablility": 1,
                                                    "PassengerType": 0,
                                                    "Value": "2004.00",
                                                    "ValueType": 0,
                                                    "DurationFrom": 5,
                                                    "DurationTo": 100,
                                                    "DurationTypeFrom": 1,
                                                    "DurationTypeTo": 1,
                                                    "OnlineServiceFee": 0,
                                                    "OfflineServiceFee": 0,
                                                    "Remarks": "5000"
                                                }
                                            ]
                                        }
                                    ]
                                }
                            ]
                        }
                    ]
                }
            ]
        }
        """;

        private const string AirRepriceResponseJson = """
        {
            "Response_Header": { "Error_Code": "0000", "Error_Desc": "SUCCESS" },
            "AirRepriceResponses": [
                {
                    "Flight": {
                        "Flight_Key": "FKEY1",
                        "Origin": "BOM",
                        "Destination": "MAA",
                        "Repriced": true,
                        "IsFareChange": true,
                        "Fares": [
                            {
                                "Fare_Id": "FARE1",
                                "Refundable": true,
                                "Seats_Available": "2",
                                "FareDetails": [
                                    { "PAX_Type": 0, "Total_Amount": 2600, "Currency_Code": "INR" }
                                ]
                            }
                        ]
                    }
                }
            ]
        }
        """;

        private const string AirFareRuleResponseJson = """
        {
            "Response_Header": { "Error_Code": "0000", "Error_Desc": "SUCCESS" },
            "FareRules": [
                { "Segment_Id": "0", "FareRuleName": "Universal", "FareRuleDesc": "<p>Cancellation allowed.</p>" }
            ]
        }
        """;

        [Fact]
        public void AirSearchResponseWire_ShouldDeserializeAllFareAndBaggageFields()
        {
            var response = JsonSerializer.Deserialize<AirSearchResponseWire>(AirSearchResponseJson)!;

            var flight = response.TripDetails.Single().Flights.Single();
            var fare = flight.Fares.Single();
            var detail = fare.FareDetails.Single();

            response.SearchKey.Should().Be("abc123");
            response.ResponseHeader!.ErrorCode.Should().Be("0000");

            flight.FlightKey.Should().Be("FKEY1");
            flight.AirlineCode.Should().Be("SG");
            flight.IsLcc.Should().BeTrue();

            var segment = flight.Segments.Single();
            segment.AirlineName.Should().Be("SpiceJet");
            segment.FlightNumber.Should().Be("6287");
            segment.DepartureDateTime.Should().Be("01/25/2022 00:50");
            segment.ArrivalDateTime.Should().Be("01/25/2022 02:40");

            fare.FareId.Should().Be("FARE1");
            fare.FareKey.Should().Be("FAREKEY1");
            fare.Refundable.Should().BeTrue();
            fare.SeatsAvailable.Should().Be("4");

            detail.PaxType.Should().Be(0);
            detail.TotalAmount.Should().Be(2487m);
            detail.CurrencyCode.Should().Be("INR");
            detail.BasicAmount.Should().Be(1924m);
            detail.AirportTaxAmount.Should().Be(563m);
            detail.AirportTaxes.Should().ContainSingle(t => t.TaxCode == "YQ" && t.TaxAmount == 100m);
            detail.FreeBaggage!.CheckInBaggage.Should().Be("15 KG");
            detail.RescheduleCharges.Should().ContainSingle(r => r.DurationFrom == 5 && r.Value == "2004.00");
        }

        [Fact]
        public void AirRepriceResponseWire_ShouldUnwrapNestedFlightEnvelope()
        {
            var response = JsonSerializer.Deserialize<AirRepriceResponseWire>(AirRepriceResponseJson)!;

            var item = response.AirRepriceResponses.Single();

            item.Flight.FlightKey.Should().Be("FKEY1");
            item.Flight.Repriced.Should().BeTrue();
            item.Flight.IsFareChange.Should().BeTrue();

            var fare = item.Flight.Fares.Single();
            fare.FareId.Should().Be("FARE1");
            fare.FareDetails.Single().TotalAmount.Should().Be(2600m);
        }

        [Fact]
        public void AirFareRuleResponseWire_ShouldDeserializeRuleText()
        {
            var response = JsonSerializer.Deserialize<AirFareRuleResponseWire>(AirFareRuleResponseJson)!;

            var rule = response.FareRules.Single();
            rule.SegmentId.Should().Be("0");
            rule.FareRuleName.Should().Be("Universal");
            rule.FareRuleDesc.Should().Contain("Cancellation allowed");
            response.ResponseHeader!.ErrorCode.Should().Be("0000");
        }
    }
}
