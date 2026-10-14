using GoVoylo.Application.Features.Authentication.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Authentication.Commands.SocialLogin
{
    // Provider: "google" | "apple" | "facebook". Token: Google/Apple ID token or
    // Facebook access token. FirstName/LastName: Apple only sends the user's name
    // to the app on the very first sign-in, never inside its token.
    public record SocialLoginCommand(
        string Provider,
        string Token,
        string? FirstName = null,
        string? LastName = null) : IRequest<LoginResponseDto>;
}
