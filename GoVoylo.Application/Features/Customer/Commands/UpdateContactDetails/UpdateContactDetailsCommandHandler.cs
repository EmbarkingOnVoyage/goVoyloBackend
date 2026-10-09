using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Application.Features.Customer.Mappers;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.UpdateContactDetails
{
    public class UpdateContactDetailsCommandHandler
        : IRequestHandler<UpdateContactDetailsCommand, CustomerProfileDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditService _auditService;
        private readonly IEncryptionService _encryptionService;

        public UpdateContactDetailsCommandHandler(
            IUserRepository userRepository,
            IAuditService auditService,
            IEncryptionService encryptionService)
        {
            _userRepository = userRepository;
            _auditService = auditService;
            _encryptionService = encryptionService;
        }

        public async Task<CustomerProfileDto> Handle(
            UpdateContactDetailsCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId)
                ?? throw new NotFoundException("Customer profile not found.");

            var email = request.Email?.Trim();
            if (!string.IsNullOrEmpty(email))
            {
                if (user.Email != null && !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    throw new BusinessRuleException(
                        "email_change_not_supported",
                        "Your sign-in email can't be changed here.");
                }

                if (user.Email == null)
                {
                    var owner = await _userRepository.GetByEmailAsync(email);
                    if (owner != null && owner.Id != user.Id)
                    {
                        throw new ConflictException(
                            "email_in_use",
                            "This email is already used by another account.");
                    }
                }
            }

            user.SetContactDetails(request.Phone.Trim(), email);
            await _userRepository.UpdateAsync(user);

            _auditService.Log(user.Id, AuditEventTypes.ProfileUpdated);

            return CustomerProfileMapper.ToDto(user, _encryptionService);
        }
    }
}
