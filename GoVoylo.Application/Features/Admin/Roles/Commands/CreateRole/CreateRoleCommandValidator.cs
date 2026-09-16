using FluentValidation;

namespace GoVoylo.Application.Features.Admin.Roles.Commands.CreateRole
{
    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .Matches(@"^[a-z][a-z0-9_]{2,49}$")
                .WithMessage("Role name must be lowercase, start with a letter, and use only letters, numbers, and underscores.");
        }
    }
}
