using GoVoylo.Application.Features.Traveler.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Queries.GetTravelerById
{
    public record GetTravelerByIdQuery(Guid UserId, Guid TravelerId) : IRequest<TravelerDetailDto>;
}
