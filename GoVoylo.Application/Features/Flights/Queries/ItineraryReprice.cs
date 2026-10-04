using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;

namespace GoVoylo.Application.Features.Flights.Queries
{
    // Reprices one offer together with the other legs of its itinerary. Tripjack's
    // Review rejects a single priceId from a domestic return or multi-city search
    // (errCode 1091 "Number of PriceId passed in the review request doesn't match
    // with the requested trips", confirmed live), so the ancillaries, seat map and
    // fare rules calls reprice every leg through one RepriceBatchAsync call, the
    // same way CreateBookingCommandHandler does. Flyshop's RepriceBatchAsync just
    // reprices each leg in turn, so nothing changes for it.
    internal static class ItineraryReprice
    {
        public sealed record RepricedLeg(Guid OfferId, int Position, FlightOfferSession Session);

        // itineraryOfferIds is every leg of the trip in display order, or empty for a
        // single-offer trip. offerId must be one of them when it isn't empty.
        public static async Task<IReadOnlyList<RepricedLeg>> RepriceAsync(
            IFlightSearchSessionStore sessionStore,
            IFlightSupplierClientResolver supplierClientResolver,
            Guid offerId,
            IReadOnlyList<Guid> itineraryOfferIds,
            CancellationToken cancellationToken)
        {
            var offerIds = itineraryOfferIds.Count > 0 ? itineraryOfferIds : new[] { offerId };
            if (!offerIds.Contains(offerId))
            {
                throw new BusinessRuleException("offer_not_in_itinerary", "The flight offer isn't part of this itinerary.");
            }

            var sessions = new List<FlightOfferSession>();
            foreach (var id in offerIds)
            {
                sessions.Add(await sessionStore.GetAsync(id, cancellationToken)
                    ?? throw new NotFoundException("Flight offer not found or has expired. Please search again."));
            }

            var supplierCode = sessions[0].SupplierCode;
            if (sessions.Any(s => s.SupplierCode != supplierCode))
            {
                throw new BusinessRuleException("mixed_supplier_itinerary", "All flights in a trip must come from the same supplier.");
            }

            var supplierClient = supplierClientResolver.Resolve(supplierCode);
            var results = await supplierClient.RepriceBatchAsync(
                sessions.Select(s => new SupplierRepriceRequestDto(s.SearchKey, s.FlightKey, s.FareId)).ToList(),
                cancellationToken);

            var legs = new List<RepricedLeg>();
            for (var i = 0; i < offerIds.Count; i++)
            {
                var updated = sessions[i] with { FlightKey = results[i].FlightKey, FareId = results[i].FareId };
                await sessionStore.UpdateAsync(offerIds[i], updated, cancellationToken);
                legs.Add(new RepricedLeg(offerIds[i], i, updated));
            }

            return legs;
        }
    }
}
