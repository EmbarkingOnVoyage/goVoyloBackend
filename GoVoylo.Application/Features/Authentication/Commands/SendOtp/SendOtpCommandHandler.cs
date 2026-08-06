using GoVoylo.Application.Features.Authentication.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using System.Security.Cryptography;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Authentication.Commands.SendOtp
{
    public class SendOtpCommandHandler
    : IRequestHandler<SendOtpCommand, SendOtpResponseDto>
    {
        private readonly IOtpRepository _otpRepository;
        private readonly IEmailService _emailService;

        public SendOtpCommandHandler(
           IOtpRepository otpRepository,
           IEmailService emailService)
        {
            _otpRepository = otpRepository;
            _emailService = emailService;
        }

        public async Task<SendOtpResponseDto> Handle(
            SendOtpCommand request,
            CancellationToken cancellationToken)
           {
            //if (existingOtp != null)
            //{
            //    //if one otp exists remove old otp.
            //    await _otpRepository.DeleteAsync(existingOtp.Id);
            //}

            // Generate OTP
            var otp = RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();

            // Generate Verification Token
            var verificationToken = Guid.NewGuid().ToString();

            // Create entity
            var otpEntity = new OtpVerification
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Otp = otp,
                VerificationToken = verificationToken,
                CreatedAt = DateTime.UtcNow,
                isVerified = false          
            };

            // Remove any existing active OTP for this email
            var existingOtp =
                await _otpRepository.GetActiveOtpByEmailAsync(request.Email);
            //If otp exist update new otp
            if (existingOtp != null)
            {
                existingOtp.Otp = otp;
                existingOtp.VerificationToken = verificationToken;
                existingOtp.CreatedAt = DateTime.UtcNow;
                existingOtp.isVerified = false;

                await _otpRepository.UpdateAsync(existingOtp);
            }
            else
            {
                await _otpRepository.SaveAsync(otpEntity);
            }

            // Send OTP email
            await _emailService.SendOtpAsync(request.Email, otp);

            // Return verification token
            return new SendOtpResponseDto
            {
                VerificationToken = verificationToken,
                Message = "OTP sent successfully."
            };
        }
    }
}
