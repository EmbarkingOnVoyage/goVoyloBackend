using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetRescheduleRules
{
    public record GetRescheduleRulesQuery(Guid SearchId, Guid OfferId) : IRequest<IReadOnlyList<RescheduleChargeDto>>;
}
