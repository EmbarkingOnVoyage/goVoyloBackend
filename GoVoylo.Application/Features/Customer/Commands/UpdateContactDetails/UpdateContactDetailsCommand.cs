using GoVoylo.Application.Features.Customer.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.UpdateContactDetails
{
    // The account's primary contact, asked for before a booking when it's missing.
    // Email is only used to fill in an account that has none.
    public record UpdateContactDetailsCommand(
        Guid UserId,
        string Phone,
        string? Email) : IRequest<CustomerProfileDto>;
}
