//using GoVoylo.Application.Features.Authentication.Dtos;
//using GoVoylo.Application.Interfaces;
//using GoVoylo.Domain.Entities;
//using GoVoylo.Domain.Interfaces;
//using MediatR;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Text;

//namespace GoVoylo.Application.Features.Authentication.Commands.RefreshTokenRefreshJWTToken
//{
//    public class RefreshTokenCommandHandler
//        : IRequestHandler<
//            RefreshTokenCommand,
//            RefreshTokenResponseDto>
//    {
//        private readonly IRefreshTokenRepository
//            _refreshTokenRepository;

//        private readonly IUserRepository
//            _userRepository;

//        private readonly IJwtTokenService
//            _jwtTokenService;

//        private readonly IRefreshTokenService
//            _refreshTokenService;

//        private readonly IUserRoleRepository _userRoleRepository;

//        private readonly IConfiguration _configuration;

//        public RefreshTokenCommandHandler(
//            IRefreshTokenRepository refreshTokenRepository,
//            IUserRepository userRepository,
//            IJwtTokenService jwtTokenService,
//            IRefreshTokenService refreshTokenService,
//                IUserRoleRepository userRoleRepository,
//            IConfiguration configuration)
//        {
//            _refreshTokenRepository =
//                refreshTokenRepository;

//            _userRepository =
//                userRepository;

//            _jwtTokenService =
//                jwtTokenService;

//            _refreshTokenService =
//                refreshTokenService;
//            _userRoleRepository = userRoleRepository;

//            _configuration = configuration;
//        }

//        //    public async Task<RefreshTokenResponseDto> Handle(
//        //        RefreshTokenCommand request,
//        //        CancellationToken cancellationToken)
//        //    {
//        //        // 1. Hash the refresh token received from client
//        //        var tokenHash =
//        //            _refreshTokenService.Hash(request.TokenRefresh);

//        //        // 2. Find refresh token in database
//        //        var storedToken =
//        //            await _refreshTokenRepository
//        //                .GetByTokenHashAsync(tokenHash);

//        //        // 3. Token doesn't exist
//        //        if (storedToken == null)
//        //        {
//        //            throw new Exception(
//        //                "Invalid refresh token.");
//        //        }

//        //        // 4. Check if token was revoked
//        //        if (storedToken.IsRevoked())
//        //        {
//        //            throw new Exception(
//        //                "Refresh token has been revoked.");
//        //        }

//        //        // 3. Token doesn't exist
//        //        if (storedToken == null) 
//        //         { 
//        //            throw new Exception("Invalid refresh token."); 
//        //        }
//        //        // 4. Check if token was revoked
//        //        if (storedToken.IsRevoked())
//        //        { 
//        //            throw new Exception("Refresh token has been revoked."); 
//        //        }

//        //        if (user == null)
//        //        {
//        //            throw new Exception(
//        //                "User not found.");
//        //        }

//        //        // 7. Check user status
//        //        if (user.Status != "active")
//        //        {
//        //            throw new Exception(
//        //                "User account is not active.");
//        //        }

//        //        // 8. Generate new access token
//        //        var newAccessToken =
//        //           _jwtTokenService.GenerateToken(user, roles);

//        //        var roles =
//        //await _userRoleRepository.GetRoleNamesForUserAsync(user.Id);

//        //        // 9. Generate new refresh token
//        //        var newRefreshToken = _refreshTokenService.GenerateRawToken();

//        //        // Refresh token expiry
//        //        var refreshTokenExpiryDays =
//        //            _configuration.GetValue<int>(
//        //                "Jwt:RefreshTokenExpiryDays");

//        //        // 10. Hash new refresh token
//        //        var newRefreshTokenHash =
//        //            _refreshTokenService
//        //                .Hash(newRefreshToken);

//        //        // 11. Revoke old refresh token
//        //        storedToken.Revoke();

//        //        await _refreshTokenRepository
//        //            .UpdateAsync(storedToken);

//        //        // 12. Create new refresh token record
//        //        //var newRefreshTokenEntity =
//        //        //    new RefreshToken(
//        //        //        user.Id,
//        //        //        newRefreshTokenHash,
//        //        //        DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
//        //        //        storedToken.DeviceInfo);
//        //        new RefreshTokenEntity(
//        //user.Id,
//        //newRefreshTokenHash,
//        //storedToken.DeviceInfo,
//        //_refreshTokenService.GetExpiryDate());

//        //        // 13. Save new refresh token
//        //        await _refreshTokenRepository
//        //            .SaveAsync(newRefreshTokenEntity);

//        //        // 14. Return both tokens
//        //        return new RefreshTokenResponseDto
//        //        {
//        //            AccessToken = newAccessToken,
//        //            RefreshToken = newRefreshTokenHash
//        //        };
//        //    }

//        public async Task<RefreshTokenResponseDto> Handle(
//            RefreshTokenCommand request,
//            CancellationToken cancellationToken)
//        {
//            // 1. Hash the refresh token received from the client
//            var tokenHash =
//                _refreshTokenService.Hash(request.TokenRefresh);

//            // 2. Find refresh token in database
//            var storedToken =
//                await _refreshTokenRepository
//                    .GetByTokenHashAsync(tokenHash);

//            // 3. Token doesn't exist
//            if (storedToken == null)
//            {
//                throw new Exception("Invalid refresh token.");
//            }

//            // 4. Check if token has been revoked
//            if (storedToken.RevokedAt != null)
//            {
//                throw new Exception("Refresh token has been revoked.");
//            }

//            // 5. Check if token has expired
//            if (storedToken.ExpiresAt <= DateTime.UtcNow)
//            {
//                throw new Exception("Refresh token has expired.");
//            }

//            // 6. Find user
//            var user =
//                await _userRepository
//                    .GetByIdAsync(storedToken.UserId);

//            if (user == null)
//            {
//                throw new Exception("User not found.");
//            }

//            // 7. Check user status
//            if (user.Status != "active")
//            {
//                throw new Exception("User account is not active.");
//            }

//            // 8. Get user roles
//            var roles =
//                await _userRoleRepository
//                    .GetRoleNamesForUserAsync(user.Id);

//            // 9. Generate new access token
//            var newAccessToken =
//                _jwtTokenService.GenerateToken(user, roles);

//            // 10. Generate new raw refresh token
//            var newRefreshToken =
//                _refreshTokenService.GenerateRawToken();

//            // 11. Hash new refresh token
//            var newRefreshTokenHash =
//                _refreshTokenService.Hash(newRefreshToken);

//            // 12. Revoke old refresh token
//            storedToken.Revoke();

//            await _refreshTokenRepository
//                .UpdateAsync(storedToken);

//            // 13. Create new refresh token entity
//            var newRefreshTokenEntity =
//                new RefreshTokenEntity(
//                    user.Id,
//                    newRefreshTokenHash,
//                    storedToken.DeviceInfo,
//                    _refreshTokenService.GetExpiryDate());

//            // 14. Save new refresh token
//            await _refreshTokenRepository
//                .SaveAsync(newRefreshTokenEntity);

//            // 15. Return tokens
//            return new RefreshTokenResponseDto
//            {
//                AccessToken = newAccessToken,
//                RefreshToken = newRefreshToken
//            };
//        }
//    }
//}


using GoVoylo.Application.Features.Authentication.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;
using RefreshTokenEntity = GoVoylo.Domain.Entities.RefreshToken;

namespace GoVoylo.Application.Features.Authentication.Commands.RefreshTokenRefreshJWTToken
{
    public class RefreshTokenCommandHandler
        : IRequestHandler<RefreshTokenCommand, RefreshTokenResponseDto>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IUserRoleRepository _userRoleRepository;

        public RefreshTokenCommandHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            IUserRoleRepository userRoleRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<RefreshTokenResponseDto> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Hash refresh token received from client
            var tokenHash =
                _refreshTokenService.Hash(
                    request.TokenRefresh);

            // 2. Find refresh token in database
            var storedToken =
                await _refreshTokenRepository
                    .GetByTokenHashAsync(tokenHash);

            // 3. Validate token existence
            if (storedToken == null)
            {
                throw new UnauthorizedAccessException(
                    "Invalid refresh token.");
            }

            // 4. Check if token has been revoked
            if (storedToken.RevokedAt != null)
            {
                throw new UnauthorizedAccessException(
                    "Refresh token has been revoked.");
            }

            // 5. Check if token has expired
            if (storedToken.ExpiresAt <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException(
                    "Refresh token has expired.");
            }

            // 6. Find user
            var user =
                await _userRepository
                    .GetByIdAsync(storedToken.UserId);

            if (user == null)
            {
                throw new UnauthorizedAccessException(
                    "User not found.");
            }

            // 7. Check user status
            if (user.Status != "active")
            {
                throw new UnauthorizedAccessException(
                    "User account is not active.");
            }

            // 8. Get user roles
            var roles =
                await _userRoleRepository
                    .GetRoleNamesForUserAsync(user.Id);

            // 9. Generate new access token
            var newAccessToken =
                _jwtTokenService.GenerateToken(
                    user,
                    roles);

            // 10. Generate new raw refresh token
            var newRefreshToken =
                _refreshTokenService.GenerateRawToken();

            // 11. Hash new refresh token
            var newRefreshTokenHash =
                _refreshTokenService.Hash(
                    newRefreshToken);

            // 12. Revoke old refresh token
            storedToken.Revoke();

            await _refreshTokenRepository
                .UpdateAsync(storedToken);

            // 13. Create new refresh token entity
            var newRefreshTokenEntity =
                new RefreshTokenEntity(
                    user.Id,
                    newRefreshTokenHash,
                    storedToken.DeviceInfo,
                    _refreshTokenService.GetExpiryDate());

            // 14. Save new refresh token
            await _refreshTokenRepository
                .SaveAsync(newRefreshTokenEntity);

            // 15. Return new tokens
            return new RefreshTokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}