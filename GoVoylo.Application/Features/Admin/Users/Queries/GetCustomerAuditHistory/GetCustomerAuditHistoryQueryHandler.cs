using GoVoylo.Application.Common;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Admin.Users.Dtos;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Admin.Users.Queries.GetCustomerAuditHistory
{
    public class GetCustomerAuditHistoryQueryHandler
        : IRequestHandler<GetCustomerAuditHistoryQuery, PagedResult<AuditHistoryEntryDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public GetCustomerAuditHistoryQueryHandler(
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository)
        {
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<PagedResult<AuditHistoryEntryDto>> Handle(
            GetCustomerAuditHistoryQuery request, CancellationToken cancellationToken)
        {
            var target = await _userRepository.GetByIdAsync(request.TargetUserId);

            if (target == null)
            {
                throw new NotFoundException("Customer not found.");
            }

            var (logs, totalCount) = await _auditLogRepository.GetByUserIdAsync(
                request.TargetUserId, request.Page, request.PageSize);

            var items = logs
                .Select(l => new AuditHistoryEntryDto(l.EventType, l.ActorUserId, l.CreatedAt))
                .ToList();

            return new PagedResult<AuditHistoryEntryDto>(items, totalCount, request.Page, request.PageSize);
        }
    }
}
