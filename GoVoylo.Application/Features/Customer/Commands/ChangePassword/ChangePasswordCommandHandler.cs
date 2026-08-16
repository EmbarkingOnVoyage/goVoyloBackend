using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler
        : IRequestHandler<ChangePasswordCommand, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IAuditService _auditService;

        public ChangePasswordCommandHandler(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IAuditService auditService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _auditService = auditService;
        }

        public async Task<Unit> Handle(
            ChangePasswordCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                throw new NotFoundException("Customer profile not found.");
            }

            if (string.IsNullOrEmpty(user.PasswordHash) ||
                !_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                throw new UnauthorizedAppException("invalid_current_password", "Current password is incorrect.");
            }

            var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
            user.ChangePasswordHash(newPasswordHash);
            await _userRepository.UpdateAsync(user);

            _auditService.Log(user.Id, AuditEventTypes.PasswordChanged);

            return Unit.Value;
        }
    }
}
