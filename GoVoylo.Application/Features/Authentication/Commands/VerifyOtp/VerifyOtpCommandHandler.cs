using GoVoylo.Application.Features.Authentication.Commands.SendOtp;
using GoVoylo.Application.Features.Authentication.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Authentication.Commands.VerifyOtp
{
    public class VerifyOtpCommandHandler 
      : IRequestHandler<VerifyOtpCommand, VerifyOtpResponseDto>
    {
        private readonly IOtpRepository _otpRepository;

        public VerifyOtpCommandHandler(IOtpRepository otpRepository)
        {
            _otpRepository = otpRepository;
        }

        public async Task<VerifyOtpResponseDto> Handle(
        VerifyOtpCommand request,
        CancellationToken cancellationToken)
        {
          //retrieve the record
          var otpRecord =
           await _otpRepository.GetByTokenAsync(
              request.VerificationToken);

            //Check whether it exists 
            if (otpRecord == null)
            {
                return new VerifyOtpResponseDto
                {
                    IsVerified = false,
                    Message = "Invalid verification token."
                };
            }

            //Check expiry
            if (DateTime.UtcNow >
             otpRecord.CreatedAt.AddMinutes(5))
            {
                return new VerifyOtpResponseDto
                {
                    IsVerified = false,
                    Message = "OTP has expired."
                };
            }

            //Compare otp
            if (otpRecord.Otp != request.Otp)
            {
                return new VerifyOtpResponseDto
                {
                    IsVerified = false,
                    Message = "Invalid OTP."
                };
            }
            otpRecord.isVerified = true;
            await _otpRepository.UpdateAsync(otpRecord);

            return new VerifyOtpResponseDto
            {
                IsVerified = true,
                Message = "OTP verified successfully."
            };

        }

        public static implicit operator VerifyOtpCommandHandler(SendOtpCommandHandler v)
        {
            throw new NotImplementedException();
        }
    }
}
