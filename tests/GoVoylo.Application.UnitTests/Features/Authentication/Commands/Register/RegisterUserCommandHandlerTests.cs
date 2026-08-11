using FluentAssertions;
using GoVoylo.Application.Features.Authentication.Commands.Register;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.UnitTests.Features.Authentication.Commands.Register
{
    public class RegisterUserCommandHandlerTests
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly RegisterCommandHandler _handler;

        public RegisterUserCommandHandlerTests()
        {
            _userRepository = Substitute.For<IUserRepository>();
            _passwordService = Substitute.For<IPasswordService>();

            _handler = new RegisterCommandHandler(
                _userRepository,
                _passwordService);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenEmailAlreadyExists()
        {
            // Arrange
            var command = new RegisterUserCommand(
                "John",
                "Doe",
                "john@gmail.com",
                null,
                "Password@123"
            );

            var existingUser = new User(
                "john@gmail.com",
                "existing-hash",
                "Existing",
                null,
                "User"
            );

            _userRepository
                .GetByEmailAsync(command.Email)
                .Returns(existingUser);

            // Act
            Func<Task> act = async () =>
                await _handler.Handle(
                    command,
                    CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Email already exists");

            await _userRepository
                .DidNotReceive()
                .SaveAsync(Arg.Any<User>());

            _passwordService
                .DidNotReceive()
                .HashPassword(Arg.Any<string>());
        }
     
        [Fact]
        public async Task Handle_ShouldReturnSameUserId_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var command = new RegisterUserCommand(
                "John",
                "Doe",
                "john@gmail.com",
                null,
                "Password@123"
            );

            _userRepository
                .GetByEmailAsync(command.Email)
                .Returns((User?)null);

            _passwordService
                .HashPassword(command.Password)
                .Returns("hashed-password");

            Guid savedUserId = Guid.Empty;

            _userRepository
                .SaveAsync(Arg.Do<User>(
                    user => savedUserId = user.Id));

            // Act
            var result = await _handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            result.Id.Should().NotBe(Guid.Empty);

            savedUserId.Should().NotBe(Guid.Empty);
            result.Id.Should().Be(savedUserId);
        }
    }
}
