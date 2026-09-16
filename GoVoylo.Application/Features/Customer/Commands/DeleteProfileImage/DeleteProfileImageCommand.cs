using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.DeleteProfileImage
{
    public record DeleteProfileImageCommand(Guid UserId) : IRequest<Unit>;
}
