using GoVoylo.Application.Features.Flights.Dtos;

namespace GoVoylo.Application.Features.Flights.Filtering
{
    // Pure, side-effect-free filtering/sorting over an already-fetched offer list —
    // shared by FilterFlightOffers and GetFilterSummary so both agree on how "stops"
    // and "duration" are derived from raw segment data.
    public static class FlightOfferFilterEngine
    {
        public static int GetStopCount(FlightOfferDto offer) => Math.Max(0, offer.Segments.Count - 1);

        public static int GetDurationMinutes(FlightOfferDto offer)
        {
            if (offer.Segments.Count == 0)
            {
                return 0;
            }

            var start = offer.Segments[0].DepartureDateTime;
            var end = offer.Segments[^1].ArrivalDateTime;
            return (int)Math.Max(0, (end - start).TotalMinutes);
        }

        public static IReadOnlyList<FlightOfferDto> Apply(
            IReadOnlyList<FlightOfferDto> offers, FlightOfferFilterRequestDto filter)
        {
            IEnumerable<FlightOfferDto> query = offers;

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(o => o.TotalAmount >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(o => o.TotalAmount <= filter.MaxPrice.Value);
            }

            if (filter.AirlineCodes is { Count: > 0 })
            {
                var airlines = new HashSet<string>(filter.AirlineCodes, StringComparer.OrdinalIgnoreCase);
                query = query.Where(o => airlines.Contains(o.AirlineCode));
            }

            if (filter.StopCounts is { Count: > 0 })
            {
                var stopCounts = new HashSet<int>(filter.StopCounts);
                query = query.Where(o => stopCounts.Contains(GetStopCount(o)));
            }

            if (filter.DepartureTimeFrom.HasValue)
            {
                query = query.Where(o => TimeOnly.FromDateTime(o.Segments[0].DepartureDateTime) >= filter.DepartureTimeFrom.Value);
            }

            if (filter.DepartureTimeTo.HasValue)
            {
                query = query.Where(o => TimeOnly.FromDateTime(o.Segments[0].DepartureDateTime) <= filter.DepartureTimeTo.Value);
            }

            if (filter.ArrivalTimeFrom.HasValue)
            {
                query = query.Where(o => TimeOnly.FromDateTime(o.Segments[^1].ArrivalDateTime) >= filter.ArrivalTimeFrom.Value);
            }

            if (filter.ArrivalTimeTo.HasValue)
            {
                query = query.Where(o => TimeOnly.FromDateTime(o.Segments[^1].ArrivalDateTime) <= filter.ArrivalTimeTo.Value);
            }

            if (filter.MaxDurationMinutes.HasValue)
            {
                query = query.Where(o => GetDurationMinutes(o) <= filter.MaxDurationMinutes.Value);
            }

            if (filter.RefundableOnly == true)
            {
                query = query.Where(o => o.Refundable);
            }

            query = filter.SortBy switch
            {
                "price_desc" => query.OrderByDescending(o => o.TotalAmount),
                "duration_asc" => query.OrderBy(GetDurationMinutes),
                "departure_asc" => query.OrderBy(o => o.Segments[0].DepartureDateTime),
                _ => query.OrderBy(o => o.TotalAmount) // "price_asc" and default
            };

            return query.ToList();
        }

        public static FilterSummaryDto Summarize(IReadOnlyList<FlightOfferDto> offers)
        {
            if (offers.Count == 0)
            {
                return new FilterSummaryDto(0, 0, [], [], 0, 0, 0);
            }

            var prices = offers.Select(o => o.TotalAmount).ToList();
            var durations = offers.Select(GetDurationMinutes).ToList();

            return new FilterSummaryDto(
                prices.Min(),
                prices.Max(),
                offers.Select(o => o.AirlineCode).Distinct().OrderBy(c => c).ToList(),
                offers.Select(GetStopCount).Distinct().OrderBy(c => c).ToList(),
                durations.Min(),
                durations.Max(),
                offers.Count);
        }
    }
}
