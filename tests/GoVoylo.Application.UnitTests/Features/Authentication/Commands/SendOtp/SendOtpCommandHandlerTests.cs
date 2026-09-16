using FluentAssertions;
using GoVoylo.Application.Features.Authentication.Commands.SendOtp;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.UnitTests.Features.Authentication.Commands.SendOtp
{
    public class SendOtpCommandHandlerTests
    {
        private readonly IOtpRepository _otpRepository;
        private readonly IEmailService _emailService;

        private readonly SendOtpCommandHandler _handler;

        public SendOtpCommandHandlerTests()
        {
            _otpRepository = Substitute.For<IOtpRepository>();

            _emailService = Substitute.For<IEmailService>();

            _handler = new SendOtpCommandHandler(
                _otpRepository,
                _emailService);
        }
 
        [Fact]
        public async Task Handle_ShouldSaveOtp_WhenNoExistingOtpExists()
        {
            // Arrange

            var command = new SendOtpCommand("shambhavi.sonwatikar@embarkingonvoyage.com");

            _otpRepository
                .GetActiveOtpByEmailAsync(command.Email)
                .Returns((OtpVerification?)null);

            // Act

            var result =
                await _handler.Handle(command, CancellationToken.None);

            // Assert

            await _otpRepository
                .Received(1)
                .SaveAsync(Arg.Any<OtpVerification>());

            await _otpRepository
                .DidNotReceive()
                .UpdateAsync(Arg.Any<OtpVerification>());

            await _emailService
                .Received(1)
                .SendOtpAsync(command.Email, Arg.Any<string>());

            result.Should().NotBeNull();

            result.Message.Should()
                .Be("OTP sent successfully.");

            result.VerificationToken.Should()
                .NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Handle_ShouldUpdateOtp_WhenOtpAlreadyExists()
        {
            // Arrange

            var command = new SendOtpCommand("shambhavi.sonwatikar@embarkingonvoyage.com");

            var existingOtp = new OtpVerification
            {
                Id = Guid.NewGuid(),
                Email = command.Email,
                Otp = "123456",
                VerificationToken = "old-token"
            };

            _otpRepository
                .GetActiveOtpByEmailAsync(command.Email)
                .Returns(existingOtp);

            // Act

            await _handler.Handle(command, CancellationToken.None);

            // Assert

            await _otpRepository
                .Received(1)
                .UpdateAsync(existingOtp);

            await _otpRepository
                .DidNotReceive()
                .SaveAsync(Arg.Any<OtpVerification>());

            await _emailService
                .Received(1)
                .SendOtpAsync(command.Email, Arg.Any<string>());
        }

        [Fact]
        public async Task Handle_ShouldSendEmail()
        {
            var command = new SendOtpCommand("shambhavi.sonwatikar@embarkingonvoyage.com");
            
            _otpRepository
                .GetActiveOtpByEmailAsync(command.Email)
                .Returns((OtpVerification?)null);

            await _handler.Handle(command, CancellationToken.None);

            await _emailService
                .Received(1)
                .SendOtpAsync(command.Email, Arg.Any<string>());
        }
    }
}
