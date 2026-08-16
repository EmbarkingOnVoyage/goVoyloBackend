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
        private readonly ISavedTravelerRepository _travelerRepository;

        public GetCustomerDashboardQueryHandler(
            IUserRepository userRepository,
            ISavedTravelerRepository travelerRepository)
        {
            _userRepository = userRepository;
            _travelerRepository = travelerRepository;
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

            var travelerCount = await _travelerRepository.CountByUserIdAsync(request.UserId);

            // Booking domain doesn't exist in this codebase yet — wire this up once it lands.
            return new CustomerDashboardDto(CustomerProfileMapper.ToDto(user), travelerCount, 0);
        }
    }
}
