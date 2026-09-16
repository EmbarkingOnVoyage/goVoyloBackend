using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.DeleteCustomerAddress
{
    public class DeleteCustomerAddressCommandHandler
        : IRequestHandler<DeleteCustomerAddressCommand, Unit>
    {
        private readonly ICustomerAddressRepository _addressRepository;

        public DeleteCustomerAddressCommandHandler(ICustomerAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<Unit> Handle(
            DeleteCustomerAddressCommand request,
            CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetByIdAsync(request.AddressId);

            if (address == null || address.UserId != request.UserId)
            {
                throw new NotFoundException("Address not found.");
            }

            if (address.IsDefault)
            {
                var remaining = await _addressRepository.CountByUserIdAsync(request.UserId);

                if (remaining > 1)
                {
                    throw new BusinessRuleException(
                        "default_address_reassignment_required",
                        "Assign another address as default before deleting this one.");
                }
            }

            await _addressRepository.DeleteAsync(address);
            return Unit.Value;
        }
    }
}
