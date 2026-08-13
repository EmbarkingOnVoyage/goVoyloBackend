using GoVoylo.Application.Features.Customer.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.UpdateCustomerProfile
{
    public record UpdateCustomerProfileCommand(
        Guid UserId,
        string FirstName,
        string LastName,
        string? Phone) : IRequest<CustomerProfileDto>;
}
