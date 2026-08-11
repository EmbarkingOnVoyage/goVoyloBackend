using GoVoylo.Application.Features.Authentication.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Authentication.Commands.Register
{
    public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string?Phone,
    string Password
) : IRequest<RegisterUserResponseDto>;
}