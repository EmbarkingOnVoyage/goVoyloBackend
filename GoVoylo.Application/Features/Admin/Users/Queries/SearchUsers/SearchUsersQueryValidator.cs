using FluentValidation;

namespace GoVoylo.Application.Features.Admin.Users.Queries.SearchUsers
{
    public class SearchUsersQueryValidator : AbstractValidator<SearchUsersQuery>
    {
        public SearchUsersQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

            RuleFor(x => x.Status)
                .Must(s => s == "active" || s == "suspended" || s == "deleted")
                .WithMessage("Status must be active, suspended, or deleted.")
                .When(x => x.Status != null);
        }
    }
}
