using FluentValidation;

namespace GoVoylo.Application.Features.Customer.Commands.AddCustomerAddress
{
    public class AddCustomerAddressCommandValidator : AbstractValidator<AddCustomerAddressCommand>
    {
        public AddCustomerAddressCommandValidator()
        {
            RuleFor(x => x.Line1).NotEmpty().MaximumLength(255);
            RuleFor(x => x.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.State).NotEmpty().MaximumLength(100);
            RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(12);
            RuleFor(x => x.Country).NotEmpty().Length(2);
            RuleFor(x => x.Label).MaximumLength(30);
        }
    }
}
