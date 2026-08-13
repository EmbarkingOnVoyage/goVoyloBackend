using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Application.Features.Customer.Mappers;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.UpdateCustomerProfile
{
    public class UpdateCustomerProfileCommandHandler
        : IRequestHandler<UpdateCustomerProfileCommand, CustomerProfileDto>
    {
        private readonly IUserRepository _userRepository;

        public UpdateCustomerProfileCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<CustomerProfileDto> Handle(
            UpdateCustomerProfileCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                throw new NotFoundException("Customer profile not found.");
            }

            user.UpdateProfile(request.FirstName, request.LastName, request.Phone);
            await _userRepository.UpdateAsync(user);

            return CustomerProfileMapper.ToDto(user);
        }
    }
}
