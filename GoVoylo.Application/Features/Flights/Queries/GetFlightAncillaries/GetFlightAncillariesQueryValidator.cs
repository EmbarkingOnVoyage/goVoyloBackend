using FluentValidation;

namespace GoVoylo.Application.Features.Flights.Queries.GetFlightAncillaries
{
    public class GetFlightAncillariesQueryValidator : AbstractValidator<GetFlightAncillariesQuery>
    {
        public GetFlightAncillariesQueryValidator()
        {
            RuleFor(x => x.OfferId).NotEmpty();
        }
    }
}
