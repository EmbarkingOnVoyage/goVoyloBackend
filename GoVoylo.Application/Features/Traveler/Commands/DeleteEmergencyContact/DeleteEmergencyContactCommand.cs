using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.DeleteEmergencyContact
{
    public record DeleteEmergencyContactCommand(Guid UserId, Guid TravelerId, Guid ContactId) : IRequest<Unit>;
}
