using FluentValidation;

namespace GoVoylo.Application.Features.Admin.Users.Queries.GetCustomerAuditHistory
{
    public class GetCustomerAuditHistoryQueryValidator : AbstractValidator<GetCustomerAuditHistoryQuery>
    {
        public GetCustomerAuditHistoryQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
