using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.DeleteCustomerAddress
{
    public record DeleteCustomerAddressCommand(Guid UserId, Guid AddressId) : IRequest<Unit>;
}
