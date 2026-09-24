using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetSeatMap
{
    public record GetSeatMapQuery(
        Guid OfferId, IReadOnlyList<SeatMapTravelerRequestDto> Travelers) : IRequest<SeatMapResponseDto>;
}
