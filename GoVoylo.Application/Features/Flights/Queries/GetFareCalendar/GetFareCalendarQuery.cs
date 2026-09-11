using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetFareCalendar
{
    public record GetFareCalendarQuery(
        string Origin, string Destination, int Month, int Year) : IRequest<FareCalendarResponseDto>;
}
