using FluentValidation;

namespace GoVoylo.Application.Features.Flights.Queries.SearchFlights
{
    public class SearchFlightsQueryValidator : AbstractValidator<SearchFlightsQuery>
    {
        public SearchFlightsQueryValidator()
        {
            RuleFor(x => x.Request.TripType).NotEmpty();
            RuleFor(x => x.Request.CabinClass).NotEmpty();
            RuleFor(x => x.Request.AdultCount).GreaterThanOrEqualTo(1);
            RuleFor(x => x.Request.ChildCount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Request.InfantCount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Request.Segments).NotEmpty();

            RuleForEach(x => x.Request.Segments).ChildRules(segment =>
            {
                segment.RuleFor(s => s.Origin).NotEmpty().Length(3);
                segment.RuleFor(s => s.Destination).NotEmpty().Length(3);
            });
        }
    }
}
