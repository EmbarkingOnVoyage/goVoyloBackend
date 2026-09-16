using FluentValidation;

namespace GoVoylo.Application.Features.Holidays.Queries.GetHolidays
{
    public class GetHolidaysQueryValidator : AbstractValidator<GetHolidaysQuery>
    {
        public GetHolidaysQueryValidator()
        {
            RuleFor(x => x.CountryCode).NotEmpty().Length(2);
            RuleFor(x => x.Year).GreaterThanOrEqualTo(2000).LessThanOrEqualTo(2100);
        }
    }
}
