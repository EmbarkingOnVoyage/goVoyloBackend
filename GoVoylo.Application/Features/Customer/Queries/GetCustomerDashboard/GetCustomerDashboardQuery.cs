using GoVoylo.Application.Features.Customer.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Queries.GetCustomerDashboard
{
    public record GetCustomerDashboardQuery(Guid UserId) : IRequest<CustomerDashboardDto>;
}
