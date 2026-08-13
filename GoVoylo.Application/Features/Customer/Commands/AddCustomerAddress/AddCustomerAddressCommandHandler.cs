using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Application.Features.Customer.Mappers;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.AddCustomerAddress
{
    public class AddCustomerAddressCommandHandler
        : IRequestHandler<AddCustomerAddressCommand, AddressDto>
    {
        private const int MaxAddressesPerCustomer = 10;

        private readonly ICustomerAddressRepository _addressRepository;

        public AddCustomerAddressCommandHandler(ICustomerAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<AddressDto> Handle(
            AddCustomerAddressCommand request,
            CancellationToken cancellationToken)
        {
            var existingCount = await _addressRepository.CountByUserIdAsync(request.UserId);

            if (existingCount >= MaxAddressesPerCustomer)
            {
                throw new BusinessRuleException(
                    "max_addresses_reached",
                    $"You can save up to {MaxAddressesPerCustomer} addresses.");
            }

            // Making this one the default is exclusive — clear any existing default first.
            if (request.IsDefault)
            {
                await _addressRepository.ClearDefaultForUserAsync(request.UserId);
            }

            var address = new CustomerAddress(
                request.UserId,
                request.Label,
                request.Line1,
                request.Line2,
                request.City,
                request.State,
                request.PostalCode,
                request.Country,
                request.IsDefault || existingCount == 0);

            await _addressRepository.AddAsync(address);

            return CustomerAddressMapper.ToDto(address);
        }
    }
}
