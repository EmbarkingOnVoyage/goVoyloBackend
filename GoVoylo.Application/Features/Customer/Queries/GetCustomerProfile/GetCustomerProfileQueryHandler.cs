using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Application.Features.Customer.Mappers;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Queries.GetCustomerProfile
{
    public class GetCustomerProfileQueryHandler
        : IRequestHandler<GetCustomerProfileQuery, CustomerProfileDto>
    {
        private readonly IUserRepository _userRepository;

        public GetCustomerProfileQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<CustomerProfileDto> Handle(
            GetCustomerProfileQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                throw new NotFoundException("Customer profile not found.");
            }

            return CustomerProfileMapper.ToDto(user);
        }
    }
}
