// GoVoylo.Infrastructure/Services/B2b/TripJack/TripJackTestService.cs
using System.Net.Http.Json;
using GoVoylo.Application.Interfaces;

namespace GoVoylo.Infrastructure.Services.B2b.TripJack;

public class TripJackTestService : ITripJackTestService
{
    private readonly HttpClient _httpClient;

    public TripJackTestService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> ExecuteRawSearchAsync(string origin, string destination, DateTime departureDate, CancellationToken ct)
    {
        // 1. Create a minimal request body based on common TripJack requirements
        var requestBody = new
        {
            searchQuery = new
            {
                cabinClass = "ECONOMY",
                paxInfo = new { ADULT = 1, CHILD = 0, INFANT = 0 },
                routeInfos = new[]
                {
                    new
                    {
                        from = origin,
                        to = destination,
                        date = departureDate.ToString("yyyy-MM-dd")
                    }
                }
            }
        };

        try
        {
            // 2. Post the payload to their standard flight search path
            // Note: We use relative path here because BaseUrl is configured globally
            var response = await _httpClient.PostAsJsonAsync("fms/v1/air-search-all", requestBody, ct);
            
            // 3. Read the complete raw content string directly (even if it's an error)
            // This is exactly what you need to see the JSON structure for front-end design
            var rawJsonString = await response.Content.ReadAsStringAsync(ct);
            
            return rawJsonString;
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"Network request failed\", \"message\": \"{ex.Message}\"}}";
        }
    }

}
