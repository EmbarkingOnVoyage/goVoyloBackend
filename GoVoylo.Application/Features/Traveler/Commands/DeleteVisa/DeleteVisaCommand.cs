using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.DeleteVisa
{
    public record DeleteVisaCommand(Guid UserId, Guid TravelerId, Guid VisaId) : IRequest<Unit>;
}
