using FluentValidation;

namespace GoVoylo.Application.Features.Flights.Queries.RepriceFlightOffer
{
    public class RepriceFlightOfferQueryValidator : AbstractValidator<RepriceFlightOfferQuery>
    {
        public RepriceFlightOfferQueryValidator()
        {
            RuleFor(x => x.OfferId).NotEmpty();
        }
    }
}
