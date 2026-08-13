using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.DeleteFrequentFlyer
{
    public record DeleteFrequentFlyerCommand(Guid UserId, Guid TravelerId, Guid FrequentFlyerId) : IRequest<Unit>;
}
