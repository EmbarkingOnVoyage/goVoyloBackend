using FluentValidation;

namespace GoVoylo.Application.Features.Flights.Queries.GetSeatMap
{
    public class GetSeatMapQueryValidator : AbstractValidator<GetSeatMapQuery>
    {
        public GetSeatMapQueryValidator()
        {
            RuleFor(x => x.OfferId).NotEmpty();
            RuleFor(x => x.Travelers).NotEmpty();

            RuleForEach(x => x.Travelers).ChildRules(traveler =>
            {
                traveler.RuleFor(t => t.FirstName).NotEmpty();
                traveler.RuleFor(t => t.LastName).NotEmpty();
            });
        }
    }
}
