using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Json;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoVoylo.Infrastructure.ExternalServices.Tripjack
{
    public class TripjackClient : IFlightSupplierClient
    {
        private readonly HttpClient _httpClient;
        private readonly TripjackOptions _options;
        private readonly ILogger<TripjackClient> _logger;

        public TripjackClient(HttpClient httpClient, IOptions<TripjackOptions> options, ILogger<TripjackClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public string SupplierCode => FlightSupplierCodes.Tripjack;

        public async Task<SupplierFlightSearchResultDto> SearchAsync(
            FlightSearchRequestDto request, CancellationToken cancellationToken)
        {
            var wireRequest = new AirSearchRequestWire
            {
                AuthHeader = BuildAuthHeader(),
                TravelType = 0,
                BookingType = MapBookingType(request.TripType),
                TripInfo = request.Segments
                    .Select((s, index) => new TripInfoWire
                    {
                        Origin = s.Origin,
                        Destination = s.Destination,
                        TravelDate = s.TravelDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                        TripId = index
                    })
                    .ToList(),
                AdultCount = request.AdultCount.ToString(CultureInfo.InvariantCulture),
                ChildCount = request.ChildCount.ToString(CultureInfo.InvariantCulture),
                InfantCount = request.InfantCount.ToString(CultureInfo.InvariantCulture),
                ClassOfTravel = MapClassOfTravel(request.CabinClass),
                InventoryType = 0,
                SourceType = 0,
                FilteredAirline = new List<FilteredAirlineWire> { new() { AirlineCode = string.Empty } }
            };

            var wireResponse = await PostAsync<AirSearchRequestWire, AirSearchResponseWire>(
                "Air_Search", wireRequest, cancellationToken);

            var flights = wireResponse.TripDetails
                .SelectMany(t => t.Flights)
                .Select(MapFlight)
                .ToList();

            return new SupplierFlightSearchResultDto(wireResponse.SearchKey, flights);
        }

        public async Task<SupplierRepriceResultDto> RepriceAsync(
            SupplierRepriceRequestDto request, CancellationToken cancellationToken)
        {
            var wireRequest = new AirRepriceRequestWire
            {
                AuthHeader = BuildAuthHeader(),
                SearchKey = request.SearchKey,
                AirRepriceRequests = new List<AirRepriceRequestItemWire>
                {
                    new() { FlightKey = request.FlightKey, FareId = request.FareId }
                },
                GstInput = false,
                SinglePricing = true
            };

            var wireResponse = await PostAsync<AirRepriceRequestWire, AirRepriceResponseWire>(
                "Air_Reprice", wireRequest, cancellationToken);

            var repriced = wireResponse.AirRepriceResponses.FirstOrDefault();

            if (repriced == null)
            {
                throw new SupplierUnavailableException("Tripjack Air_Reprice returned no repriced flight.");
            }

            var mapped = MapFlight(repriced);

            return new SupplierRepriceResultDto(
                mapped.FlightKey,
                mapped.FareId,
                mapped.TotalAmount,
                mapped.CurrencyCode,
                repriced.Repriced,
                repriced.IsFareChange);
        }

        private AuthHeaderWire BuildAuthHeader() => new()
        {
            UserId = _options.UserId,
            Password = _options.Password,
            IpAddress = _options.IpAddress,
            RequestId = Guid.NewGuid().ToString("N"),
            ImeiNumber = _options.ImeiNumber
        };

        private static SupplierFlightOptionDto MapFlight(FlightWire flight)
        {
            var primaryFare = flight.Fares.FirstOrDefault();

            var adultFareDetail = primaryFare?.FareDetails.FirstOrDefault(f => f.PaxType == "0")
                ?? primaryFare?.FareDetails.FirstOrDefault();

            return new SupplierFlightOptionDto(
                flight.FlightKey,
                primaryFare?.FareId ?? string.Empty,
                flight.AirlineCode ?? flight.Segments.FirstOrDefault()?.AirlineCode ?? string.Empty,
                flight.Segments.FirstOrDefault()?.AirlineName ?? string.Empty,
                ParseBool(primaryFare?.Refundable),
                flight.IsLcc,
                flight.Segments.Select(MapSegment).ToList(),
                adultFareDetail?.TotalAmount ?? 0m,
                adultFareDetail?.CurrencyCode ?? "INR",
                ParseInt(primaryFare?.SeatsAvailable));
        }

        private static SupplierFlightSegmentDto MapSegment(SegmentWire segment) => new(
            segment.Origin,
            segment.Destination,
            segment.AirlineCode,
            segment.FlightNumber,
            ParseDateTime(segment.DepartureDateTime),
            ParseDateTime(segment.ArrivalDateTime),
            segment.Duration);

        private static int MapBookingType(string tripType) => tripType switch
        {
            "OneWay" => 0,
            "RoundTrip" => 1,
            "MultiCity" => 3,
            _ => 0
        };

        private static string MapClassOfTravel(string cabinClass) => cabinClass switch
        {
            "Business" => "1",
            "First" => "2",
            "PremiumEconomy" => "3",
            _ => "0"
        };

        private static bool ParseBool(string? value) =>
            value is not null && (value.Equals("Y", StringComparison.OrdinalIgnoreCase)
                || value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value == "1");

        private static int ParseInt(string? value) =>
            int.TryParse(value, out var parsed) ? parsed : 0;

        private static DateTime ParseDateTime(string? value) =>
            DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed
                : default;

        private async Task<TResponse> PostAsync<TRequest, TResponse>(
            string method, TRequest body, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var httpResponse = await _httpClient.PostAsJsonAsync(method, body, cancellationToken);
                stopwatch.Stop();

                _logger.LogInformation(
                    "Tripjack {Method} responded {StatusCode} in {ElapsedMs}ms",
                    method, (int)httpResponse.StatusCode, stopwatch.ElapsedMilliseconds);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new SupplierUnavailableException(
                        $"Tripjack {method} returned {(int)httpResponse.StatusCode} {httpResponse.StatusCode}.");
                }

                var result = await httpResponse.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);

                return result ?? throw new SupplierUnavailableException($"Tripjack {method} returned an empty response.");
            }
            catch (SupplierUnavailableException)
            {
                throw;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or TimeoutException or InvalidOperationException)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex, "Tripjack {Method} failed after {ElapsedMs}ms", method, stopwatch.ElapsedMilliseconds);
                throw new SupplierUnavailableException($"Could not reach Tripjack ({method}).");
            }
        }
    }
}
