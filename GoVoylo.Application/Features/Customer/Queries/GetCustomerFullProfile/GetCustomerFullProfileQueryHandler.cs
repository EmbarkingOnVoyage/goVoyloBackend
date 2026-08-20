using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Application.Features.Customer.Mappers;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Queries.GetCustomerFullProfile
{
    public class GetCustomerFullProfileQueryHandler
        : IRequestHandler<GetCustomerFullProfileQuery, CustomerFullProfileDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICustomerAddressRepository _addressRepository;
        private readonly ICustomerGstDetailRepository _gstRepository;
        private readonly IUserPreferenceRepository _preferenceRepository;
        private readonly INotificationPreferenceRepository _notificationPreferenceRepository;
        private readonly IEncryptionService _encryptionService;

        public GetCustomerFullProfileQueryHandler(
            IUserRepository userRepository,
            ICustomerAddressRepository addressRepository,
            ICustomerGstDetailRepository gstRepository,
            IUserPreferenceRepository preferenceRepository,
            INotificationPreferenceRepository notificationPreferenceRepository,
            IEncryptionService encryptionService)
        {
            _userRepository = userRepository;
            _addressRepository = addressRepository;
            _gstRepository = gstRepository;
            _preferenceRepository = preferenceRepository;
            _notificationPreferenceRepository = notificationPreferenceRepository;
            _encryptionService = encryptionService;
        }

        public async Task<CustomerFullProfileDto> Handle(
            GetCustomerFullProfileQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                throw new NotFoundException("Customer profile not found.");
            }

            var addressesTask = _addressRepository.GetByUserIdAsync(request.UserId);
            var gstTask = _gstRepository.GetByUserIdAsync(request.UserId);
            var preferenceTask = _preferenceRepository.GetByUserIdAsync(request.UserId);
            var notificationPreferenceTask = _notificationPreferenceRepository.GetByUserIdAsync(request.UserId);

            await Task.WhenAll(addressesTask, gstTask, preferenceTask, notificationPreferenceTask);

            var addresses = addressesTask.Result.Select(CustomerAddressMapper.ToDto).ToList();

            var gst = gstTask.Result;
            var gstDto = gst == null
                ? null
                : new GstDetailsDto(gst.Id, gst.Gstin, gst.LegalName, gst.TradeName, gst.IsVerified);

            var preference = preferenceTask.Result;
            var preferencesDto = preference == null
                ? new PreferencesDto("en", "INR")
                : new PreferencesDto(preference.Language, preference.Currency);

            var notificationPreference = notificationPreferenceTask.Result;
            var notificationPreferencesDto = notificationPreference == null
                ? new NotificationPreferencesDto(true, true, true, false, true)
                : new NotificationPreferencesDto(
                    notificationPreference.EmailTransactional,
                    notificationPreference.EmailMarketing,
                    notificationPreference.SmsTransactional,
                    notificationPreference.SmsMarketing,
                    notificationPreference.PushEnabled);

            return new CustomerFullProfileDto(
                CustomerProfileMapper.ToDto(user, _encryptionService),
                addresses,
                gstDto,
                preferencesDto,
                notificationPreferencesDto);
        }
    }
}
