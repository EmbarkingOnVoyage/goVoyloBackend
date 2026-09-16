using FluentAssertions;
using GoVoylo.Application.Features.Authentication.Commands.VerifyOtp;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.UnitTests.Features.Authentication.Commands.VerifyOtp
{
    public class VerifyOtpCommandHandlerTest
    {
        private readonly IOtpRepository _otpRepository;
        private readonly VerifyOtpCommandHandler _handler;

        public VerifyOtpCommandHandlerTest()
        {
            _otpRepository = Substitute.For<IOtpRepository>();

            _handler = new VerifyOtpCommandHandler(_otpRepository);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenOtpIsValid()
        {
            // Arrange

            var command = new VerifyOtpCommand(
                "john@gmail.com",
                "verification-token",
                "654321");

            var otpRecord = new OtpVerification
            {
                Id = Guid.NewGuid(),
                Email = "john@gmail.com",
                Otp = "654321",
                VerificationToken = "verification-token",
                CreatedAt = DateTime.UtcNow,
                isVerified = false
            };

            _otpRepository
                .GetByTokenAsync(command.VerificationToken)
                .Returns(otpRecord);

            // Act

            var result = await _handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();

            result.IsVerified.Should().BeTrue();

            result.Message.Should()
                .Be("OTP verified successfully.");

            otpRecord.isVerified.Should().BeTrue();

            await _otpRepository
                .Received(1)
                .UpdateAsync(otpRecord);
        }

        [Fact]
        public async Task Handle_ShouldReturnInvalidToken_WhenTokenDoesNotExist()
        {
            // Arrange
            var command = new VerifyOtpCommand(
                "john@gmail.com",
                "invalid-token",
                "654321");

            _otpRepository
                .GetByTokenAsync(command.VerificationToken)
                .Returns((OtpVerification?)null);

            // Act
            var result = await _handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();

            result.IsVerified.Should().BeFalse();

            result.Message.Should()
                .Be("Invalid verification token.");

            await _otpRepository
                .DidNotReceive()
                .UpdateAsync(Arg.Any<OtpVerification>());
        }
    }
}

