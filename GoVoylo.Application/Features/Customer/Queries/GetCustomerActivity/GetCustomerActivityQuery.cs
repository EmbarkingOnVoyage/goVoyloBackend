using GoVoylo.Application.Features.Customer.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Queries.GetCustomerActivity
{
    public record GetCustomerActivityQuery(Guid UserId) : IRequest<IReadOnlyList<ActivityLogDto>>;
}
