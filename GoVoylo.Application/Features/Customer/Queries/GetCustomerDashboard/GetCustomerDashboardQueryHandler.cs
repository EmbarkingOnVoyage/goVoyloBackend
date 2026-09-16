using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Application.Features.Customer.Mappers;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Queries.GetCustomerDashboard
{
    public class GetCustomerDashboardQueryHandler
        : IRequestHandler<GetCustomerDashboardQuery, CustomerDashboardDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly ISavedTravelerRepository _travelerRepository;
        private readonly IEncryptionService _encryptionService;

        public GetCustomerDashboardQueryHandler(
            IUserRepository userRepository,
            ISavedTravelerRepository travelerRepository,
            IEncryptionService encryptionService)
        {
            _userRepository = userRepository;
            _travelerRepository = travelerRepository;
            _encryptionService = encryptionService;
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
            return new CustomerDashboardDto(CustomerProfileMapper.ToDto(user, _encryptionService), travelerCount, 0);
        }
    }
}
