using FluentValidation;

namespace GoVoylo.Application.Features.Flights.Queries.GetFareRules
{
    public class GetFareRulesQueryValidator : AbstractValidator<GetFareRulesQuery>
    {
        public GetFareRulesQueryValidator()
        {
            RuleFor(x => x.OfferId).NotEmpty();
        }
    }
}
