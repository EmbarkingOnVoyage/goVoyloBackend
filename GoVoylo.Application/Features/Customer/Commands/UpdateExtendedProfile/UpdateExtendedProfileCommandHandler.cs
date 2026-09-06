using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Application.Features.Customer.Mappers;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.UpdateExtendedProfile
{
    public class UpdateExtendedProfileCommandHandler
        : IRequestHandler<UpdateExtendedProfileCommand, CustomerProfileDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditService _auditService;
        private readonly IEncryptionService _encryptionService;

        public UpdateExtendedProfileCommandHandler(
            IUserRepository userRepository,
            IAuditService auditService,
            IEncryptionService encryptionService)
        {
            _userRepository = userRepository;
            _auditService = auditService;
            _encryptionService = encryptionService;
        }

        public async Task<CustomerProfileDto> Handle(
            UpdateExtendedProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                throw new NotFoundException("Customer profile not found.");
            }

            // The client only ever receives a masked passport/PAN number (see
            // CustomerProfileMapper), never the real value, so it has no way to
            // "resend" an unchanged one. Treat an empty field as "not editing this"
            // and preserve the existing encrypted value instead of wiping it —
            // otherwise every profile save that doesn't touch these two fields
            // would silently delete the customer's stored passport/PAN.
            var passportChanged = !string.IsNullOrWhiteSpace(request.PassportNumber);
            var passportNumberEncrypted = passportChanged
                ? _encryptionService.Encrypt(request.PassportNumber)
                : user.PassportNumberEncrypted;
            var passportExpiryDate = passportChanged ? request.PassportExpiryDate : user.PassportExpiryDate;
            var passportIssuingCountry = passportChanged ? request.PassportIssuingCountry : user.PassportIssuingCountry;

            var panCardNumberEncrypted = string.IsNullOrWhiteSpace(request.PanCardNumber)
                ? user.PanCardNumberEncrypted
                : _encryptionService.Encrypt(request.PanCardNumber);

            user.UpdateExtendedProfile(
                request.Gender,
                request.DateOfBirth,
                request.Nationality,
                request.MaritalStatus,
                request.Anniversary,
                request.CityOfResidence,
                request.State,
                passportNumberEncrypted,
                passportExpiryDate,
                passportIssuingCountry,
                panCardNumberEncrypted,
                request.AutoAddTravelInsurance);

            await _userRepository.UpdateAsync(user);

            _auditService.Log(user.Id, AuditEventTypes.ProfileUpdated);

            return CustomerProfileMapper.ToDto(user, _encryptionService);
        }
    }
}
