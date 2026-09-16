using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Application.Features.Customer.Mappers;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.UpdateCustomerAddress
{
    public class UpdateCustomerAddressCommandHandler
        : IRequestHandler<UpdateCustomerAddressCommand, AddressDto>
    {
        private readonly ICustomerAddressRepository _addressRepository;

        public UpdateCustomerAddressCommandHandler(ICustomerAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<AddressDto> Handle(
            UpdateCustomerAddressCommand request,
            CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetByIdAsync(request.AddressId);

            if (address == null || address.UserId != request.UserId)
            {
                throw new NotFoundException("Address not found.");
            }

            if (request.IsDefault && !address.IsDefault)
            {
                await _addressRepository.ClearDefaultForUserAsync(request.UserId);
                address.SetAsDefault(true);
            }

            address.Update(
                request.Label,
                request.Line1,
                request.Line2,
                request.City,
                request.State,
                request.PostalCode,
                request.Country);

            await _addressRepository.UpdateAsync(address);

            return CustomerAddressMapper.ToDto(address);
        }
    }
}
