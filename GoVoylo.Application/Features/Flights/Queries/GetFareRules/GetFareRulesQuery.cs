using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetFareRules
{
    // FareIds: the fare picked for each offer, same order — see ItineraryReprice.
    public record GetFareRulesQuery(IReadOnlyList<Guid> OfferIds, IReadOnlyList<string?>? FareIds = null)
        : IRequest<FareRulesResponseDto>;
}
