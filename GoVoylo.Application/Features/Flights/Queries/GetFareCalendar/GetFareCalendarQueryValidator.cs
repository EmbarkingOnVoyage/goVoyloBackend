using FluentValidation;

namespace GoVoylo.Application.Features.Flights.Queries.GetFareCalendar
{
    public class GetFareCalendarQueryValidator : AbstractValidator<GetFareCalendarQuery>
    {
        public GetFareCalendarQueryValidator()
        {
            RuleFor(x => x.Origin).NotEmpty().Length(3);
            RuleFor(x => x.Destination).NotEmpty().Length(3);
            RuleFor(x => x.Month).InclusiveBetween(1, 12);
            RuleFor(x => x.Year).GreaterThanOrEqualTo(DateTime.UtcNow.Year);
        }
    }
}
