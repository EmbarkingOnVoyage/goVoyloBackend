using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Authentication.Dtos;
using GoVoylo.Application.Features.Authentication.Services;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Common;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;
using RefreshTokenEntity = GoVoylo.Domain.Entities.RefreshToken;

namespace GoVoylo.Application.Features.Authentication.Commands.SocialLogin
{
    // "Continue with Google / Apple / Facebook". The provider vouches for who the
    // user is; from there it's the same as an email-OTP login: find the account
    // (by the linked provider id, else by the provider's verified email), create
    // one on first sign-in, merge guest bookings, issue tokens.
    public class SocialLoginCommandHandler : IRequestHandler<SocialLoginCommand, LoginResponseDto>
    {
        private readonly ISocialTokenVerifier _verifier;
        private readonly IAuthIdentityRepository _authIdentityRepository;
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IAuditService _auditService;
        private readonly GuestAccountMerger _guestAccountMerger;

        public SocialLoginCommandHandler(
            ISocialTokenVerifier verifier,
            IAuthIdentityRepository authIdentityRepository,
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            IRefreshTokenRepository refreshTokenRepository,
            IUserRoleRepository userRoleRepository,
            IRoleRepository roleRepository,
            IAuditService auditService,
            GuestAccountMerger guestAccountMerger)
        {
            _verifier = verifier;
            _authIdentityRepository = authIdentityRepository;
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _auditService = auditService;
            _guestAccountMerger = guestAccountMerger;
        }

        public async Task<LoginResponseDto> Handle(SocialLoginCommand request, CancellationToken cancellationToken)
        {
            VerifiedSocialIdentity identity;
            try
            {
                identity = await _verifier.VerifyAsync(request.Provider, request.Token, cancellationToken);
            }
            catch (UnauthorizedAppException)
            {
                _auditService.Log(null, AuditEventTypes.LoginFailed);
                throw;
            }

            // 1. An account this provider id is already linked to.
            User? user = null;
            var link = await _authIdentityRepository.GetAsync(identity.Provider, identity.Subject, cancellationToken);
            if (link != null)
            {
                user = await _userRepository.GetByIdAsync(link.UserId);
            }

            if (user == null)
            {
                // 2. First sign-in with this provider: the account with the same
                // (provider-verified) email, else a new one.
                if (string.IsNullOrWhiteSpace(identity.Email))
                {
                    throw new BusinessRuleException(
                        "email_required",
                        "We couldn't get an email address from this account. Please continue with your email instead.");
                }

                var email = identity.Email.Trim().ToLowerInvariant();
                user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    var firstName = FirstNonBlank(request.FirstName, identity.FirstName) ?? email.Split('@')[0];
                    var lastName = FirstNonBlank(request.LastName, identity.LastName) ?? string.Empty;
                    user = new User(email, firstName, lastName);
                    await _userRepository.SaveAsync(user);

                    var customerRole = await _roleRepository.GetByNameAsync("customer");
                    if (customerRole != null)
                    {
                        await _userRoleRepository.AssignAsync(new UserRole(user.Id, customerRole.Id));
                    }

                    _auditService.Log(user.Id, AuditEventTypes.Registration);
                }

                if (link == null)
                {
                    await _authIdentityRepository.AddAsync(
                        new AuthIdentity(user.Id, identity.Provider, identity.Subject), cancellationToken);
                }
            }

            if (user.Status != "active")
            {
                _auditService.Log(user.Id, AuditEventTypes.LoginFailed);
                throw new ForbiddenException("account_inactive", "User account is not active.");
            }

            // Bookings made as a guest with this email now belong to the account.
            await _guestAccountMerger.MergeIntoAsync(user, cancellationToken);

            var roles = await _userRoleRepository.GetRoleNamesForUserAsync(user.Id);
            var accessToken = _jwtTokenService.GenerateToken(user, roles);

            var rawRefreshToken = _refreshTokenService.GenerateRawToken();
            await _refreshTokenRepository.SaveAsync(new RefreshTokenEntity(
                user.Id,
                _refreshTokenService.Hash(rawRefreshToken),
                deviceInfo: null,
                _refreshTokenService.GetExpiryDate()));

            _auditService.Log(user.Id, AuditEventTypes.LoginSuccess);

            return new LoginResponseDto
            {
                Id = user.Id,
                Message = "Login successful.",
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken
            };
        }

        private static string? FirstNonBlank(params string?[] values) =>
            values.Select(v => v?.Trim()).FirstOrDefault(v => !string.IsNullOrEmpty(v));
    }
}
