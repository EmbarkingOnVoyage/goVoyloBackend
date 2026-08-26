using FluentValidation;

namespace GoVoylo.Application.Features.Admin.Users.Commands.UpdateCustomerStatus
{
    public class UpdateCustomerStatusCommandValidator : AbstractValidator<UpdateCustomerStatusCommand>
    {
        public UpdateCustomerStatusCommandValidator()
        {
            RuleFor(x => x.Status)
                .Must(s => s == "active" || s == "suspended")
                .WithMessage("Status must be active or suspended. Use the account-deletion endpoint to deactivate permanently.");
        }
    }
}
