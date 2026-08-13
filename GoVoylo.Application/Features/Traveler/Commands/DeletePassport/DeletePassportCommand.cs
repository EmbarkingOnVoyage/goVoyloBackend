using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.DeletePassport
{
    public record DeletePassportCommand(Guid UserId, Guid TravelerId) : IRequest<Unit>;
}
