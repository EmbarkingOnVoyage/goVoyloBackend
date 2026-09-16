using GoVoylo.Application.Interfaces;

namespace GoVoylo.Infrastructure.Services.B2b.TripJack;

public class TripJackMockService : ITripJackTestService
{
    public async Task<string> ExecuteRawSearchAsync(string origin, string destination, DateTime departureDate, CancellationToken ct)
    {
        // Simulated network delay (150ms) to ensure frontend spinners can be tested
        await Task.Delay(150, ct);

        // Production-accurate TripJack v3 API replica dataset containing all display constraints
        var productionMockJson = $$"""
        {
          "status": "SUCCESS",
          "metaData": {
            "searchId": "TJ-SRCH-994821-XYZ",
            "provider": "TripJack_B2B_Gate",
            "executionTimeMs": 142
          },
          "searchResults": [
            {
              "flightId": "TJ-AI-2026-805",
              "airlineInfo": {
                "code": "AI",
                "name": "Air India",
                "logoUrl": "https://govoylo.com"
              },
              "flightNumber": "AI-805",
              "cabinClass": "ECONOMY",
              "bookingClass": "N",
              "availableSeats": 7, 
              "isRefundable": true, 
              "fareType": "PUBLISHED", 
              "route": {
                "originAirport": "{{origin}}",
                "destinationAirport": "{{destination}}",
                "departureTime": "{{departureDate.ToString("yyyy-MM-dd")}}T06:00:00",
                "arrivalTime": "{{departureDate.ToString("yyyy-MM-dd")}}T08:15:00",
                "durationMinutes": 135,
                "stops": 0
              },
              "pricing": {
                "currency": "INR",
                "supplierBaseFare": 4500.00,
                "supplierTax": 850.00,
                "b2bAgentCommission": 250.00, 
                "suggestedAgentMarkup": 150.00, 
                "totalPublishedPrice": 5350.00 
              },
              "baggageRules": {
                "cabinBaggage": "7 KG",
                "checkInBaggage": "15 KG",
                "excessBaggageAvailable": true
              },
              "policyHighlights": {
                "cancellationFee": "INR 3000 if cancelled 24hrs before departure",
                "dateChangeFee": "INR 2500 + Fare Difference",
                "isFreeMealIncluded": false,
                "isSeatSelectionChargeable": true
              }
            },
            {
              "flightId": "TJ-6E-2026-512",
              "airlineInfo": {
                "code": "6E",
                "name": "IndiGo",
                "logoUrl": "https://govoylo.com"
              },
              "flightNumber": "6E-512",
              "cabinClass": "ECONOMY",
              "bookingClass": "E",
              "availableSeats": 2, 
              "isRefundable": false, 
              "fareType": "SME_SPECIAL", 
              "route": {
                "originAirport": "{{origin}}",
                "destinationAirport": "{{destination}}",
                "departureTime": "{{departureDate.ToString("yyyy-MM-dd")}}T14:30:00",
                "arrivalTime": "{{departureDate.ToString("yyyy-MM-dd")}}T16:50:00",
                "durationMinutes": 140,
                "stops": 0
              },
              "pricing": {
                "currency": "INR",
                "supplierBaseFare": 3900.00,
                "supplierTax": 750.00,
                "b2bAgentCommission": 110.00,
                "suggestedAgentMarkup": 200.00,
                "totalPublishedPrice": 4650.00
              },
              "baggageRules": {
                "cabinBaggage": "7 KG",
                "checkInBaggage": "15 KG",
                "excessBaggageAvailable": false
              },
              "policyHighlights": {
                "cancellationFee": "Non-Refundable Ticket",
                "dateChangeFee": "INR 3250 + Fare Difference",
                "isFreeMealIncluded": true,
                "isSeatSelectionChargeable": false
              }
            }
          ]
        }
        """;

        return productionMockJson;
    }
}
