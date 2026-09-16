using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Admin.Users.Commands.UpdateCustomerStatus
{
    public class UpdateCustomerStatusCommandHandler
        : IRequestHandler<UpdateCustomerStatusCommand, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditService _auditService;

        public UpdateCustomerStatusCommandHandler(
            IUserRepository userRepository,
            IAuditService auditService)
        {
            _userRepository = userRepository;
            _auditService = auditService;
        }

        public async Task<Unit> Handle(UpdateCustomerStatusCommand request, CancellationToken cancellationToken)
        {
            if (request.TargetUserId == request.AdminUserId)
            {
                throw new BusinessRuleException(
                    "cannot_change_own_status",
                    "You cannot activate or deactivate your own account through this endpoint.");
            }

            var target = await _userRepository.GetByIdAsync(request.TargetUserId);

            if (target == null)
            {
                throw new NotFoundException("Customer not found.");
            }

            if (request.Status == "active")
            {
                target.Activate();
            }
            else
            {
                target.Suspend();
            }

            await _userRepository.UpdateAsync(target);

            _auditService.Log(
                userId: target.Id,
                eventType: AuditEventTypes.CustomerStatusChanged,
                actorUserId: request.AdminUserId);

            return Unit.Value;
        }
    }
}
