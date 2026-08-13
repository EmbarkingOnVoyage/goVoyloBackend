using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Application.Features.Customer.Mappers;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Queries.GetCustomerDashboard
{
    public class GetCustomerDashboardQueryHandler
        : IRequestHandler<GetCustomerDashboardQuery, CustomerDashboardDto>
    {
        private readonly IUserRepository _userRepository;

        public GetCustomerDashboardQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<CustomerDashboardDto> Handle(
            GetCustomerDashboardQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                throw new NotFoundException("Customer profile not found.");
            }

            // SavedTraveler and Booking domains don't exist in this codebase yet —
            // wire these up once those repositories land.
            return new CustomerDashboardDto(CustomerProfileMapper.ToDto(user), 0, 0);
        }
    }
}
