using GoVoylo.Application.Common;
using GoVoylo.Application.Features.Admin.Users.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Admin.Users.Queries.GetCustomerAuditHistory
{
    public record GetCustomerAuditHistoryQuery(
        Guid TargetUserId,
        int Page,
        int PageSize) : IRequest<PagedResult<AuditHistoryEntryDto>>;
}
