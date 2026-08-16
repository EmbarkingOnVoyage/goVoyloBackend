using FluentValidation;

namespace GoVoylo.Application.Features.Admin.Roles.Commands.UpdateRole
{
    public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
    {
        public UpdateRoleCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .Matches(@"^[a-z][a-z0-9_]{2,49}$")
                .WithMessage("Role name must be lowercase, start with a letter, and use only letters, numbers, and underscores.");
        }
    }
}
