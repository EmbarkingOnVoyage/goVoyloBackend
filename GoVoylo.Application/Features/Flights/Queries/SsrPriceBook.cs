using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;

namespace GoVoylo.Application.Features.Flights.Queries
{
    // Remembers the price of every add-on and seat offered for a flight, by SSR
    // key, on its search session — the booking only sends the keys, and the
    // convenience fee includes what the add-ons cost.
    internal static class SsrPriceBook
    {
        public static Task RecordAsync(
            IFlightSearchSessionStore sessionStore, Guid offerId, FlightOfferSession session,
            IEnumerable<(string? Key, decimal Amount)> options, CancellationToken cancellationToken)
        {
            var prices = new Dictionary<string, decimal>(session.SsrPrices ?? new Dictionary<string, decimal>());
            var changed = false;
            foreach (var (key, amount) in options)
            {
                if (string.IsNullOrEmpty(key) || (prices.TryGetValue(key, out var known) && known == amount)) continue;
                prices[key] = amount;
                changed = true;
            }

            return changed
                ? sessionStore.UpdateAsync(offerId, session with { SsrPrices = prices }, cancellationToken)
                : Task.CompletedTask;
        }

        public static decimal PriceOf(FlightOfferSession session, string ssrKey) =>
            session.SsrPrices != null && session.SsrPrices.TryGetValue(ssrKey, out var amount) ? amount : 0m;
    }
}
