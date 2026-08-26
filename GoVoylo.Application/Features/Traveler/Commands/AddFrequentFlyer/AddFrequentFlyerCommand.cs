using GoVoylo.Application.Features.Traveler.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.AddFrequentFlyer
{
    public record AddFrequentFlyerCommand(
        Guid UserId,
        Guid TravelerId,
        string AirlineCode,
        string MembershipNumber) : IRequest<FrequentFlyerDto>;
}
