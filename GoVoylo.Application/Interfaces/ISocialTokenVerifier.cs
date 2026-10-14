namespace GoVoylo.Application.Interfaces
{
    // Who a social sign-in token belongs to, once the provider has vouched for it.
    // Email is null when the provider didn't share one (or it isn't verified).
    public record VerifiedSocialIdentity(
        string Provider,
        string Subject,
        string? Email,
        string? FirstName,
        string? LastName);

    public interface ISocialTokenVerifier
    {
        // Google / Apple: the ID token (JWT). Facebook: the user access token.
        // Throws UnauthorizedAppException when the token isn't valid for GoVoylo.
        Task<VerifiedSocialIdentity> VerifyAsync(string provider, string token, CancellationToken cancellationToken);
    }
}
