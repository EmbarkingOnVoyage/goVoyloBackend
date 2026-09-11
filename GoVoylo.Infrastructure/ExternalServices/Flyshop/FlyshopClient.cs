using System.Globalization;
using System.Net.Http.Json;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using Microsoft.Extensions.Options;

namespace GoVoylo.Infrastructure.ExternalServices.Flyshop
{
    public class FlyshopClient : IFlightSupplierClient
    {
        private readonly HttpClient _httpClient;
        private readonly FlyshopOptions _options;

        public FlyshopClient(HttpClient httpClient, IOptions<FlyshopOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public string SupplierCode => FlightSupplierCodes.Flyshop;

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

            EnsureSuccess(wireResponse.ResponseHeader, "Air_Search");

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
                CustomerMobile = _options.CustomerMobile,
                GstInput = false,
                SinglePricing = true
            };

            var wireResponse = await PostAsync<AirRepriceRequestWire, AirRepriceResponseWire>(
                "Air_Reprice", wireRequest, cancellationToken);

            EnsureSuccess(wireResponse.ResponseHeader, "Air_Reprice");

            var repriced = wireResponse.AirRepriceResponses.FirstOrDefault()?.Flight;

            if (repriced == null)
            {
                throw new InvalidOperationException("Flyshop Air_Reprice returned no repriced flight.");
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

        public async Task<SupplierLowFareResultDto> GetLowFareCalendarAsync(
            SupplierLowFareRequestDto request, CancellationToken cancellationToken)
        {
            var wireRequest = new AirLowFareRequestWire
            {
                AuthHeader = BuildAuthHeader(),
                Origin = request.Origin,
                Destination = request.Destination,
                Month = request.Month.ToString("D2", CultureInfo.InvariantCulture),
                Year = request.Year
            };

            var wireResponse = await PostAsync<AirLowFareRequestWire, AirLowFareResponseWire>(
                "Air_LowFare", wireRequest, cancellationToken);

            EnsureSuccess(wireResponse.ResponseHeader, "Air_LowFare");

            var days = wireResponse.LowFares
                .Select(f => new SupplierLowFareDayDto(
                    ParseDate(f.TravelDate),
                    f.Amount,
                    "INR",
                    f.AirlineCode ?? string.Empty,
                    f.AirlinesName ?? string.Empty))
                .Where(d => d.TravelDate != default)
                .ToList();

            return new SupplierLowFareResultDto(days);
        }

        private static DateTime ParseDate(string? value) =>
            DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed
                : default;

        private AuthHeaderWire BuildAuthHeader() => new()
        {
            UserId = _options.UserId,
            Password = _options.Password,
            IpAddress = _options.IpAddress,
            RequestId = Guid.NewGuid().ToString("N"),
            ImeiNumber = _options.ImeiNumber
        };

        private static void EnsureSuccess(ResponseHeaderWire? header, string method)
        {
            if (header == null)
            {
                return;
            }

            // "0000" is the collection's documented success code; anything else carries
            // an Error_Desc worth surfacing instead of failing deserialization silently.
            if (!string.IsNullOrEmpty(header.ErrorCode) && header.ErrorCode != "0000")
            {
                throw new InvalidOperationException(
                    $"Flyshop {method} returned {header.ErrorCode}: {header.ErrorDesc}");
            }
        }

        private static SupplierFlightOptionDto MapFlight(FlightWire flight)
        {
            var primaryFare = flight.Fares.FirstOrDefault();

            var adultFareDetail = primaryFare?.FareDetails.FirstOrDefault(f => f.PaxType == 0)
                ?? primaryFare?.FareDetails.FirstOrDefault();

            return new SupplierFlightOptionDto(
                flight.FlightKey,
                primaryFare?.FareId ?? string.Empty,
                flight.AirlineCode ?? flight.Segments.FirstOrDefault()?.AirlineCode ?? string.Empty,
                flight.Segments.FirstOrDefault()?.AirlineName ?? string.Empty,
                primaryFare?.Refundable ?? false,
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

        private static int ParseInt(string? value) =>
            int.TryParse(value, out var parsed) ? parsed : 0;

        private static DateTime ParseDateTime(string? value) =>
            DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed
                : default;

        private async Task<TResponse> PostAsync<TRequest, TResponse>(
            string method, TRequest body, CancellationToken cancellationToken)
        {
            using var httpResponse = await _httpClient.PostAsJsonAsync(method, body, cancellationToken);
            httpResponse.EnsureSuccessStatusCode();

            var result = await httpResponse.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);

            return result ?? throw new InvalidOperationException($"Flyshop {method} returned an empty response.");
        }
    }
}
