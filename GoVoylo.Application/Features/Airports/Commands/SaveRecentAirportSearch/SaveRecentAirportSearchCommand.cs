using MediatR;

namespace GoVoylo.Application.Features.Airports.Commands.SaveRecentAirportSearch
{
    // Internal — invoked after a flight search when the caller is authenticated, not
    // exposed as its own endpoint. Anonymous searches simply don't call this.
    public record SaveRecentAirportSearchCommand(Guid UserId, string IataCode) : IRequest<Unit>;
}
