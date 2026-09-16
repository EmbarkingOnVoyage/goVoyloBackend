using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.DeleteCustomerAccount
{
    public record DeleteCustomerAccountCommand(Guid UserId) : IRequest<Unit>;
}
