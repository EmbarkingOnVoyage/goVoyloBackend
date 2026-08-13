using GoVoylo.Application.Features.Customer.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.UpdateCustomerAddress
{
    public record UpdateCustomerAddressCommand(
        Guid UserId,
        Guid AddressId,
        string? Label,
        string Line1,
        string? Line2,
        string City,
        string State,
        string PostalCode,
        string Country,
        bool IsDefault) : IRequest<AddressDto>;
}
