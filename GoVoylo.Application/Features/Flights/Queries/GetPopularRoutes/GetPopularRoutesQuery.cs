using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetPopularRoutes
{
    public record GetPopularRoutesQuery : IRequest<IReadOnlyList<RouteDto>>;
}
