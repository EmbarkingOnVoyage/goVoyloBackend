using FluentValidation;

namespace GoVoylo.Application.Features.Customer.Commands.AddGstDetails
{
    public class AddGstDetailsCommandValidator : AbstractValidator<AddGstDetailsCommand>
    {
        // Standard Indian GSTIN shape: 2-digit state code + 10-char PAN + entity code + 'Z' + checksum.
        private const string GstinPattern = @"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z][1-9A-Z]Z[0-9A-Z]$";

        public AddGstDetailsCommandValidator()
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
