using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetFareRules
{
    public record GetFareRulesQuery(IReadOnlyList<Guid> OfferIds) : IRequest<FareRulesResponseDto>;
}
