using GoVoylo.Application.Features.Customer.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Queries.GetCustomerProfile
{
    public record GetCustomerProfileQuery(Guid UserId) : IRequest<CustomerProfileDto>;
}
