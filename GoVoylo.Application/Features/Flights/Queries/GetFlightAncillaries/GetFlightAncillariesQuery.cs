using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetFlightAncillaries
{
    public record GetFlightAncillariesQuery(Guid OfferId) : IRequest<FlightAncillariesResponseDto>;
}
