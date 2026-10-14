namespace GoVoylo.Infrastructure.ExternalServices.SocialAuth
{
    // "SocialAuth" in configuration. A provider with no client ids configured is
    // turned off (its sign-ins are refused).
    public class SocialAuthOptions
    {
        // OAuth client ids whose ID tokens are accepted (web client, Android/iOS clients).
        public string[] GoogleClientIds { get; set; } = Array.Empty<string>();

        // Apple Services ID (web / Android) and the iOS bundle id.
        public string[] AppleClientIds { get; set; } = Array.Empty<string>();

        public string? FacebookAppId { get; set; }
        public string? FacebookAppSecret { get; set; }
    }
}
