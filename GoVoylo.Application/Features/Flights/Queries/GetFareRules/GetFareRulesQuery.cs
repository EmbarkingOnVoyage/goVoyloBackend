using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetFareRules
{
    public record GetFareRulesQuery(Guid OfferId) : IRequest<FareRulesResponseDto>;
}
