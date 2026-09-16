using GoVoylo.Application.Features.Customer.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.AddCustomerAddress
{
    public record AddCustomerAddressCommand(
        Guid UserId,
        string? Label,
        string Line1,
        string? Line2,
        string City,
        string State,
        string PostalCode,
        string Country,
        bool IsDefault) : IRequest<AddressDto>;
}
