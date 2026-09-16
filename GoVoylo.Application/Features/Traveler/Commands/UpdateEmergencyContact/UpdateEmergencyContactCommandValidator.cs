using FluentValidation;

namespace GoVoylo.Application.Features.Traveler.Commands.UpdateEmergencyContact
{
    public class UpdateEmergencyContactCommandValidator : AbstractValidator<UpdateEmergencyContactCommand>
    {
        public UpdateEmergencyContactCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Relationship).MaximumLength(50);
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
            RuleFor(x => x.PhoneCountryCode).NotEmpty().MaximumLength(5);
            RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        }
    }
}
