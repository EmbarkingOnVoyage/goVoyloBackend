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

            // The client only sends PassportNumber/PanCardNumber when the user actually
            // retyped them — omitted (null) means "leave whatever's already saved alone",
            // not "clear it". Falling back to the user's existing encrypted values here
            // is what makes that contract hold; overwriting with null on every other
            // profile save (name, DOB, marital status, ...) was wiping out passport/PAN
            // data the user had entered in a previous save.
            var passportNumberEncrypted = string.IsNullOrWhiteSpace(request.PassportNumber)
                ? user.PassportNumberEncrypted
                : _encryptionService.Encrypt(request.PassportNumber);

            var panCardNumberEncrypted = string.IsNullOrWhiteSpace(request.PanCardNumber)
                ? user.PanCardNumberEncrypted
                : _encryptionService.Encrypt(request.PanCardNumber);

            var passportExpiryDate = string.IsNullOrWhiteSpace(request.PassportNumber)
                ? user.PassportExpiryDate
                : request.PassportExpiryDate;

            var passportIssuingCountry = string.IsNullOrWhiteSpace(request.PassportNumber)
                ? user.PassportIssuingCountry
                : request.PassportIssuingCountry;

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
