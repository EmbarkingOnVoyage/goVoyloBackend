using GoVoylo.Application.Features.Traveler.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Queries.GetTravelers
{
    public record GetTravelersQuery(Guid UserId) : IRequest<IReadOnlyList<TravelerDto>>;
}
