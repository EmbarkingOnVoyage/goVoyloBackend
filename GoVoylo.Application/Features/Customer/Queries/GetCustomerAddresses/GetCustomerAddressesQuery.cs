using GoVoylo.Application.Features.Customer.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Queries.GetCustomerAddresses
{
    public record GetCustomerAddressesQuery(Guid UserId) : IRequest<IReadOnlyList<AddressDto>>;
}
