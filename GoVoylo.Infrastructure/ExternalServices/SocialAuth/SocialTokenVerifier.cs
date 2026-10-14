using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace GoVoylo.Infrastructure.ExternalServices.SocialAuth
{
    // Checks a social sign-in token with its provider:
    // - Google / Apple ID tokens: signature against the provider's published keys
    //   (OpenID discovery, cached and refreshed by ConfigurationManager), issuer,
    //   audience (one of our client ids) and expiry.
    // - Facebook access tokens: Graph debug_token must say it was issued to our
    //   app, then /me gives the user's id, name and email.
    public class SocialTokenVerifier : ISocialTokenVerifier
    {
        private static readonly ConfigurationManager<OpenIdConnectConfiguration> GoogleConfig = new(
            "https://accounts.google.com/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever());

        private static readonly ConfigurationManager<OpenIdConnectConfiguration> AppleConfig = new(
            "https://appleid.apple.com/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever());

        private readonly HttpClient _httpClient;
        private readonly SocialAuthOptions _options;
        private readonly ILogger<SocialTokenVerifier> _logger;

        public SocialTokenVerifier(HttpClient httpClient, IOptions<SocialAuthOptions> options, ILogger<SocialTokenVerifier> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public Task<VerifiedSocialIdentity> VerifyAsync(string provider, string token, CancellationToken cancellationToken) =>
            provider switch
            {
                "google" => VerifyIdTokenAsync(provider, token, GoogleConfig,
                    new[] { "https://accounts.google.com", "accounts.google.com" }, _options.GoogleClientIds, cancellationToken),
                "apple" => VerifyIdTokenAsync(provider, token, AppleConfig,
                    new[] { "https://appleid.apple.com" }, _options.AppleClientIds, cancellationToken),
                "facebook" => VerifyFacebookAsync(token, cancellationToken),
                _ => throw Rejected(),
            };

        private async Task<VerifiedSocialIdentity> VerifyIdTokenAsync(
            string provider,
            string token,
            ConfigurationManager<OpenIdConnectConfiguration> config,
            string[] issuers,
            string[] audiences,
            CancellationToken cancellationToken)
        {
            if (audiences.Length == 0)
            {
                throw NotEnabled(provider);
            }

            var discovery = await config.GetConfigurationAsync(cancellationToken);
            var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, new TokenValidationParameters
            {
                ValidIssuers = issuers,
                ValidAudiences = audiences,
                IssuerSigningKeys = discovery.SigningKeys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
            });

            if (!result.IsValid)
            {
                // Signed with a key we haven't fetched yet (the provider rotated
                // keys): refetch so the user's retry succeeds.
                if (result.Exception is SecurityTokenSignatureKeyNotFoundException)
                {
                    config.RequestRefresh();
                }
                _logger.LogWarning(result.Exception, "{Provider} sign-in token rejected.", provider);
                throw Rejected();
            }

            var claims = result.Claims;
            var subject = claims.TryGetValue("sub", out var sub) ? sub?.ToString() : null;
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw Rejected();
            }

            // Only an email the provider has verified is used to find an account.
            var email = claims.TryGetValue("email", out var e) ? e?.ToString() : null;
            var verified = claims.TryGetValue("email_verified", out var v)
                && string.Equals(v?.ToString(), "true", StringComparison.OrdinalIgnoreCase);

            return new VerifiedSocialIdentity(
                provider,
                subject,
                verified ? email : null,
                claims.TryGetValue("given_name", out var given) ? given?.ToString() : null,
                claims.TryGetValue("family_name", out var family) ? family?.ToString() : null);
        }

        private async Task<VerifiedSocialIdentity> VerifyFacebookAsync(string accessToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.FacebookAppId) || string.IsNullOrWhiteSpace(_options.FacebookAppSecret))
            {
                throw NotEnabled("facebook");
            }

            var appToken = $"{_options.FacebookAppId}|{_options.FacebookAppSecret}";
            using var debug = await GetJsonAsync(
                $"https://graph.facebook.com/debug_token?input_token={Uri.EscapeDataString(accessToken)}&access_token={Uri.EscapeDataString(appToken)}",
                cancellationToken);
            var data = debug.RootElement.TryGetProperty("data", out var d) ? d : default;
            if (data.ValueKind != JsonValueKind.Object
                || !data.TryGetProperty("is_valid", out var valid) || !valid.GetBoolean()
                || !data.TryGetProperty("app_id", out var appId) || appId.GetString() != _options.FacebookAppId)
            {
                throw Rejected();
            }

            // appsecret_proof ties the call to our app secret.
            var proof = Convert.ToHexString(
                HMACSHA256.HashData(Encoding.UTF8.GetBytes(_options.FacebookAppSecret), Encoding.UTF8.GetBytes(accessToken))).ToLowerInvariant();
            using var me = await GetJsonAsync(
                $"https://graph.facebook.com/me?fields=id,first_name,last_name,email&access_token={Uri.EscapeDataString(accessToken)}&appsecret_proof={proof}",
                cancellationToken);
            var root = me.RootElement;
            string? Text(string name) => root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;

            var id = Text("id");
            if (string.IsNullOrWhiteSpace(id))
            {
                throw Rejected();
            }
            // Facebook only returns a confirmed email.
            return new VerifiedSocialIdentity("facebook", id, Text("email"), Text("first_name"), Text("last_name"));
        }

        private async Task<JsonDocument> GetJsonAsync(string url, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Facebook Graph call failed: {Status}", (int)response.StatusCode);
                throw Rejected();
            }
            return JsonDocument.Parse(body);
        }

        private static UnauthorizedAppException Rejected() =>
            new("invalid_social_token", "We couldn't verify that sign-in. Please try again.");

        private static UnauthorizedAppException NotEnabled(string provider) =>
            new("social_login_unavailable", $"Sign in with {char.ToUpperInvariant(provider[0])}{provider[1..]} isn't available yet.");
    }
}
