using GoVoylo.Application.Features.Authentication.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Authentication.Commands.StartGuestSession
{
    // "Continue as guest" at checkout: the email (where the e-ticket goes, and
    // how the booking is later found by signing in) and the booking mobile.
    public record StartGuestSessionCommand(
        string Email,
        string Phone) : IRequest<LoginResponseDto>;
}
