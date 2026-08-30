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
        private const string SuccessErrorCode = "0000";

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

            var repriced = wireResponse.AirRepriceResponses.FirstOrDefault()?.Flight;

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

        public async Task<SupplierFareRulesResultDto> GetFareRulesAsync(
            string searchKey, string flightKey, string fareId, CancellationToken cancellationToken)
        {
            var wireRequest = new AirFareRuleRequestWire
            {
                AuthHeader = BuildAuthHeader(),
                SearchKey = searchKey,
                FlightKey = flightKey,
                FareId = fareId
            };

            var wireResponse = await PostAsync<AirFareRuleRequestWire, AirFareRuleResponseWire>(
                "Air_FareRule", wireRequest, cancellationToken);

            var rules = wireResponse.FareRules
                .Select(r => new SupplierFareRuleDto(
                    r.SegmentId ?? string.Empty, r.FareRuleName ?? string.Empty, r.FareRuleDesc ?? string.Empty))
                .ToList();

            return new SupplierFareRulesResultDto(rules);
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
                ParseInt(primaryFare?.SeatsAvailable),
                MapFareBreakdown(adultFareDetail),
                MapBaggage(adultFareDetail),
                MapRescheduleCharges(adultFareDetail));
        }

        private static SupplierFlightSegmentDto MapSegment(SegmentWire segment) => new(
            segment.Origin,
            segment.Destination,
            segment.AirlineCode,
            segment.FlightNumber,
            ParseDateTime(segment.DepartureDateTime),
            ParseDateTime(segment.ArrivalDateTime),
            segment.Duration);

        private static SupplierFareBreakdownDto MapFareBreakdown(FareDetailWire? fareDetail) => new(
            fareDetail?.BasicAmount ?? 0m,
            fareDetail?.AirportTaxAmount ?? 0m,
            fareDetail?.AirportTaxes
                .Select(t => new SupplierFareTaxDto(t.TaxCode ?? string.Empty, t.TaxDesc ?? string.Empty, t.TaxAmount))
                .ToList()
                ?? new List<SupplierFareTaxDto>(),
            fareDetail?.ServiceFeeAmount ?? 0m,
            fareDetail?.TradeMarkupAmount ?? 0m,
            fareDetail?.PromoDiscount ?? 0m,
            fareDetail?.Gst ?? 0m,
            fareDetail?.Tds ?? 0m,
            fareDetail?.TotalAmount ?? 0m,
            fareDetail?.CurrencyCode ?? "INR");

        private static SupplierBaggageDto MapBaggage(FareDetailWire? fareDetail) => new(
            fareDetail?.FreeBaggage?.CheckInBaggage,
            fareDetail?.FreeBaggage?.HandBaggage);

        private static IReadOnlyList<SupplierRescheduleChargeDto> MapRescheduleCharges(FareDetailWire? fareDetail) =>
            fareDetail?.RescheduleCharges
                .Select(r => new SupplierRescheduleChargeDto(
                    r.PassengerType,
                    r.Value,
                    r.ValueType,
                    r.DurationFrom,
                    r.DurationTo,
                    r.DurationTypeFrom,
                    r.DurationTypeTo,
                    r.OnlineServiceFee,
                    r.OfflineServiceFee,
                    r.Remarks))
                .ToList()
                ?? new List<SupplierRescheduleChargeDto>();

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
            where TResponse : ITripjackEnvelope
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

                if (result == null)
                {
                    throw new SupplierUnavailableException($"Tripjack {method} returned an empty response.");
                }

                // Tripjack signals business-level failures inside a 200 OK body via
                // Response_Header.Error_Code rather than an HTTP error status.
                var errorCode = result.ResponseHeader?.ErrorCode;
                if (errorCode is not null && errorCode != SuccessErrorCode)
                {
                    throw new SupplierUnavailableException(
                        $"Tripjack {method} returned {errorCode} {result.ResponseHeader?.ErrorDesc}.");
                }

                return result;
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
