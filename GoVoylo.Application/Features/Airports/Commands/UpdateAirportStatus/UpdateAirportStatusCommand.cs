using GoVoylo.Application.Features.Airports.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Airports.Commands.UpdateAirportStatus
{
    public record UpdateAirportStatusCommand(string IataCode, bool IsActive) : IRequest<AirportDto>;
}
