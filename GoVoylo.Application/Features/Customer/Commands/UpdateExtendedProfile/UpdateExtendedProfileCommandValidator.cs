using FluentValidation;

namespace GoVoylo.Application.Features.Customer.Commands.UpdateExtendedProfile
{
    public class UpdateExtendedProfileCommandValidator : AbstractValidator<UpdateExtendedProfileCommand>
    {
        public UpdateExtendedProfileCommandValidator()
        {
            RuleFor(x => x.Gender).MaximumLength(20);
            RuleFor(x => x.Nationality).MaximumLength(50);
            RuleFor(x => x.MaritalStatus).MaximumLength(20);
            RuleFor(x => x.CityOfResidence).MaximumLength(100);
            RuleFor(x => x.State).MaximumLength(100);
            RuleFor(x => x.PassportIssuingCountry).MaximumLength(50);

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.UtcNow.Date)
                .When(x => x.DateOfBirth.HasValue);

            RuleFor(x => x.Anniversary)
                .LessThanOrEqualTo(DateTime.UtcNow.Date)
                .When(x => x.Anniversary.HasValue);

            RuleFor(x => x.PassportNumber).MaximumLength(20);

            RuleFor(x => x.PassportExpiryDate)
                .NotNull()
                .GreaterThan(DateTime.UtcNow.Date)
                .WithMessage("Passport has already expired.")
                .When(x => !string.IsNullOrWhiteSpace(x.PassportNumber));

            RuleFor(x => x.PanCardNumber)
                .Matches("^[A-Z]{5}[0-9]{4}[A-Z]{1}$")
                .WithMessage("PAN card number must be in the format ABCDE1234F.")
                .When(x => !string.IsNullOrWhiteSpace(x.PanCardNumber));
        }
    }
}
