using FluentValidation;

namespace GoVoylo.Application.Features.Flights.Queries.GetRescheduleRules
{
    public class GetRescheduleRulesQueryValidator : AbstractValidator<GetRescheduleRulesQuery>
    {
        public GetRescheduleRulesQueryValidator()
        {
            RuleFor(x => x.SearchId).NotEmpty();
            RuleFor(x => x.OfferId).NotEmpty();
        }
    }
}
