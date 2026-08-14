using FluentAssertions;
using GoVoylo.Application.Features.Authentication.Commands.ResetPassword;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace GoVoylo.Application.UnitTests.Features.Authentication.Commands.Resetpassword
{
    public class ResetpasswordCommandHandlerTest
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly ResetPasswordCommandHandler _handler;

        public ResetpasswordCommandHandlerTest()
        {
            _userRepository = Substitute.For<IUserRepository>();
            _passwordService = Substitute.For<IPasswordService>();
            _handler = new ResetPasswordCommandHandler(_userRepository , _passwordService);
        }

        [Fact]
        public async Task Handle_ShouldChangePassword_WhenOldPasswordIsCorrect()
        {
            // Arrange
            var command = new ResetPasswordCommand(
                "john@gmail.com",
                "OldPassword@123",
                "NewPassword@456");

            var user = new User(
                "john@gmail.com",
                "old-password-hash",
                "9876543210",
                "John",
                "Doe");

            _userRepository
                .GetByEmailAsync(command.Email)
                .Returns(user);

            _passwordService
                .VerifyPassword(
                    command.OldPassword,
                    user.PasswordHash!)
                .Returns(true);

            _passwordService
                .HashPassword(command.NewPassword)
                .Returns("new-password-hash");

            // Act
            var result = await _handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();

            result.Message
                .Should()
                .Be("Password changed successfully.");

            _passwordService
                .Received(1)
                .VerifyPassword(
                    command.OldPassword,
                    "old-password-hash");

            _passwordService
                .Received(1)
                .HashPassword(
                    command.NewPassword);

            await _userRepository
                .Received(1)
                .UpdateAsync(user);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenOldPasswordIsIncorrect()
        {
            // Arrange
            var command = new ResetPasswordCommand(
                "john@gmail.com",
                "WrongPassword@123",
                "NewPassword@456");

            var user = new User(
                "john@gmail.com",
                "old-password-hash",
                "9876543210",
                "John",
                "Doe");

            _userRepository
                .GetByEmailAsync(command.Email)
                .Returns(user);

            _passwordService
                .VerifyPassword(
                    command.OldPassword,
                    user.PasswordHash!)
                .Returns(false);

            //// Act
            //Func<Task> act = async () =>
            //    await _handler.Handle(
            //        command,
            //        CancellationToken.None);

            //// Assert
            //await act.Should()
            //    .ThrowAsync<Exception>()
            //    .WithMessage("Old password is incorrect.");

            //_passwordService
            //    .DidNotReceive()
            //    .HashPassword(
            //        Arg.Any<string>());

            //await _userRepository
            //    .DidNotReceive()
            //    .UpdateAsync(
            //        Arg.Any<User>());

            // Act + Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _handler.Handle(
                    command,
                    CancellationToken.None));

            // Verify that the handler rejected the wrong password
            exception.Message
                .Should()
                .Be("Old password is incorrect");

            // New password should NOT be hashed
            _passwordService
                .DidNotReceive()
                .HashPassword(
                    Arg.Any<string>());

            // User should NOT be updated
            await _userRepository
                .DidNotReceive()
                .UpdateAsync(
                    Arg.Any<User>());
        }
    }
}
