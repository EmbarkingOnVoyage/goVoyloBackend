using GoVoylo.Application.Features.Authentication.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Authentication.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler: IRequestHandler<ResetPasswordCommand, ResetPasswordResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;

        public ResetPasswordCommandHandler(IUserRepository userRepository, IPasswordService passwordService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
        }

        public async Task<ResetPasswordResponseDto>Handle(ResetPasswordCommand request,CancellationToken cancellation)
        {
            //Find bu user email
            var user = await _userRepository.GetByEmailAsync(request.Email);

            //User doesnt exists
            if(user == null)
            {
                throw new Exception("Invalid email or password");
            }

            // Check account status
            if (user.Status != "active")
            {
                throw new Exception(
                    "User account is not active.");
            }

            //Verify old password
            if (string.IsNullOrEmpty(user.PasswordHash)||
                !_passwordService.VerifyPassword(request.OldPassword,
                user.PasswordHash)
                )
            {
                throw new Exception("Old password is incorrect");
            }

            //Hash new password 
            var newPasswordHash =
            _passwordService.HashPassword(
                request.NewPassword);

            //update password
            user.ResetPassword(newPasswordHash);

            // 7. Save user
            await _userRepository.UpdateAsync(user);

            return new ResetPasswordResponseDto
            {
                Message = "Password changed successfully."
            };
        }
    }
}
