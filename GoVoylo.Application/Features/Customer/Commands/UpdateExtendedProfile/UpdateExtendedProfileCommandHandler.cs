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

            var passportNumberEncrypted = string.IsNullOrWhiteSpace(request.PassportNumber)
                ? null
                : _encryptionService.Encrypt(request.PassportNumber);

            var panCardNumberEncrypted = string.IsNullOrWhiteSpace(request.PanCardNumber)
                ? null
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
                passportNumberEncrypted == null ? null : request.PassportExpiryDate,
                passportNumberEncrypted == null ? null : request.PassportIssuingCountry,
                panCardNumberEncrypted,
                request.AutoAddTravelInsurance);

            await _userRepository.UpdateAsync(user);

            _auditService.Log(user.Id, AuditEventTypes.ProfileUpdated);

            return CustomerProfileMapper.ToDto(user, _encryptionService);
        }
    }
}
