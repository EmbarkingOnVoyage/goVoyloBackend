using GoVoylo.Application.Features.Customer.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Queries.GetCustomerFullProfile
{
    public record GetCustomerFullProfileQuery(Guid UserId) : IRequest<CustomerFullProfileDto>;
}
