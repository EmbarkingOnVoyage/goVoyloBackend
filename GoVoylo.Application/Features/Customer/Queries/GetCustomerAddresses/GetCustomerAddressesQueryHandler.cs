using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Application.Features.Customer.Mappers;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Queries.GetCustomerAddresses
{
    public class GetCustomerAddressesQueryHandler
        : IRequestHandler<GetCustomerAddressesQuery, IReadOnlyList<AddressDto>>
    {
        private readonly ICustomerAddressRepository _addressRepository;

        public GetCustomerAddressesQueryHandler(ICustomerAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<IReadOnlyList<AddressDto>> Handle(
            GetCustomerAddressesQuery request,
            CancellationToken cancellationToken)
        {
            var addresses = await _addressRepository.GetByUserIdAsync(request.UserId);
            return addresses.Select(CustomerAddressMapper.ToDto).ToList();
        }
    }
}
