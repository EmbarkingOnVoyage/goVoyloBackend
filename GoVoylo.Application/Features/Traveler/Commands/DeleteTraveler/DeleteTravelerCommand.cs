using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.DeleteTraveler
{
    public record DeleteTravelerCommand(Guid UserId, Guid TravelerId) : IRequest<Unit>;
}
