using FluentValidation;

namespace GoVoylo.Application.Features.Customer.Commands.UpdateGstDetails
{
    public class UpdateGstDetailsCommandValidator : AbstractValidator<UpdateGstDetailsCommand>
    {
        private const string GstinPattern = @"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z][1-9A-Z]Z[0-9A-Z]$";

        public UpdateGstDetailsCommandValidator()
        {
            RuleFor(x => x.Gstin)
                .NotEmpty()
                .Matches(GstinPattern)
                .WithMessage("Invalid GSTIN format.");

            RuleFor(x => x.LegalName)
                .NotEmpty()
                .MaximumLength(255);

            RuleFor(x => x.TradeName)
                .MaximumLength(255);
        }
    }
}
