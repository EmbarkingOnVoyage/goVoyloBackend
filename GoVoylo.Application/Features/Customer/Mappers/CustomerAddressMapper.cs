using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Domain.Entities;

namespace GoVoylo.Application.Features.Customer.Mappers
{
    public static class CustomerAddressMapper
    {
        public static AddressDto ToDto(CustomerAddress address)
        {
            return new AddressDto(
                address.Id,
                address.Label,
                address.Line1,
                address.Line2,
                address.City,
                address.State,
                address.PostalCode,
                address.Country,
                address.IsDefault);
        }
    }
}
