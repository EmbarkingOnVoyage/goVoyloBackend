using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetFilterSummary
{
    public record GetFilterSummaryQuery(Guid SearchId) : IRequest<FilterSummaryDto>;
}
