using FluentAssertions;
using GoVoylo.Application.Features.Authentication.Commands.RefreshTokenRefreshJWTToken;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using RefreshTokenEntity = GoVoylo.Domain.Entities.RefreshToken;

namespace GoVoylo.Application.UnitTests.Features.Authentication.Commands.RefreshJWTToken
{
    public class RefreshTokenCommandHandlerTests
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository; 
        private readonly IUserRepository _userRepository; 
        private readonly IJwtTokenService _jwtTokenService; 
        private readonly IRefreshTokenService _refreshTokenService; 
        private readonly IConfiguration _configuration;

        private readonly RefreshTokenCommandHandler _handler;


        public RefreshTokenCommandHandlerTests() 
        {
            _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>(); 
            _userRepository = Substitute.For<IUserRepository>(); 
            _jwtTokenService = Substitute.For<IJwtTokenService>(); 
            _refreshTokenService = Substitute.For<IRefreshTokenService>();
            //_configuration = Substitute.For<IConfiguration>(); 
            _configuration =
       new ConfigurationBuilder()
           .AddInMemoryCollection(new Dictionary<string, string?>
           {
               ["Jwt:RefreshTokenExpiryDays"] = "1"
           })
           .Build();
            _handler = new RefreshTokenCommandHandler
                (_refreshTokenRepository, 
                 _userRepository, 
                 _jwtTokenService, 
                 _refreshTokenService, 
                 _configuration
               );
        }

        [Fact]
        public async Task Handle_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
        {
            // Arrange
            var command = new RefreshTokenCommand(
                "old-refresh-token");

            var user = new User(
                "john@gmail.com",
                "hashed-password",
                "9876543210",
                "John",
                "Doe"
                );

            var storedToken = new RefreshTokenEntity(
                user.Id,
                "old-token-hash",
                DateTime.UtcNow.AddDays(5),
                "Chrome");

            var newRefreshToken = "new-refresh-token";
            var newRefreshTokenHash = "new-refresh-token-hash";
            var newAccessToken = "new-access-token";

            _refreshTokenService
                .HashToken(command.TokenRefresh)
                .Returns("old-token-hash");

            _refreshTokenRepository
                .GetByTokenHashAsync("old-token-hash")
                .Returns(storedToken);

            _userRepository
                .GetByIdAsync(storedToken.UserId)
                .Returns(user);

            _jwtTokenService
                .GenerateToken(user)
                .Returns(newAccessToken);

            _refreshTokenService
                .GenerateRefreshToken()
                .Returns(newRefreshToken);

            _refreshTokenService
                .HashToken(newRefreshToken)
                .Returns(newRefreshTokenHash);

            _configuration["Jwt:RefreshTokenExpiryDays"]
                .Returns("1");

            // Act
            var result = await _handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();

            result.AccessToken
                .Should()
                .Be(newAccessToken);

            result.RefreshToken
                .Should()
                .Be(newRefreshToken);

            await _refreshTokenRepository
                .Received(1)
                .GetByTokenHashAsync("old-token-hash");

            await _userRepository
                .Received(1)
                .GetByIdAsync(storedToken.UserId);

            _jwtTokenService
                .Received(1)
                .GenerateToken(user);

            _refreshTokenService
                .Received(1)
                .GenerateRefreshToken();

            await _refreshTokenRepository
                .Received(1)
                .UpdateAsync(storedToken);

            await _refreshTokenRepository
                .Received(1)
                .SaveAsync(
                    Arg.Is<RefreshTokenEntity>(token =>
                        token.UserId == user.Id &&
                        token.TokenHash == newRefreshTokenHash &&
                        token.DeviceInfo == storedToken.DeviceInfo));
        }


        [Fact] public async Task Handle_ShouldThrowException_WhenRefreshTokenIsRevoked() 
        { 
        // Arrange
        //Create request
          var command = new RefreshTokenCommand( "revoked-refresh-token");

            // creating a fake refresh-token record that represents something like this in your database:
            var storedToken = new RefreshToken(
              Guid.NewGuid(), 
              "token-hash", 
              DateTime.UtcNow.AddDays(5),
              "Chrome"
              ); 
           
            //revoke token
            storedToken.Revoke(); 

            //
            _refreshTokenService
                .HashToken(command.TokenRefresh)
                .Returns("token-hash");

            //Finds record in database
            _refreshTokenRepository
                .GetByTokenHashAsync("token-hash")
                .Returns(storedToken);

            // Act

            //runs your refresh-token logic.
            Func<Task> act = async () => await 
            _handler.Handle( 
                command, 
                CancellationToken.None);

            // Assert
            //Verify correct message
            await act.Should() 
                .ThrowAsync<Exception>() 
                .WithMessage("Refresh token has been revoked.");

            await _userRepository 
                .DidNotReceive() 
                .GetByIdAsync(Arg.Any<Guid>());
            
            _jwtTokenService 
                .DidNotReceive() 
                .GenerateToken(Arg.Any<User>());
            
            await _refreshTokenRepository 
                .DidNotReceive() 
                .SaveAsync(Arg.Any<RefreshToken>());
        }
    }
}
