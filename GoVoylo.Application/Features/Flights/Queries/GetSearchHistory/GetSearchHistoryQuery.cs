using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetSearchHistory
{
    public record GetSearchHistoryQuery(Guid UserId) : IRequest<IReadOnlyList<SearchHistoryDto>>;
}
