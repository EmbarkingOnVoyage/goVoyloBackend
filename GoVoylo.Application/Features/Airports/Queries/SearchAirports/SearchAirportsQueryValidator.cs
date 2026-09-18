using FluentValidation;

namespace GoVoylo.Application.Features.Airports.Queries.SearchAirports
{
    public class SearchAirportsQueryValidator : AbstractValidator<SearchAirportsQuery>
    {
        public SearchAirportsQueryValidator()
        {
            RuleFor(x => x.Query)
                .NotEmpty()
                .MinimumLength(2)
                .WithMessage("Enter at least 2 characters to search airports.");
        }
    }
}
