using GoVoylo.Application.Features.Authentication.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Authentication.Commands.StartGuestSession
{
    public class StartGuestSessionCommandHandler : IRequestHandler<StartGuestSessionCommand, LoginResponseDto>
    {
        // Long enough to search, book and pay in one sitting; there's no refresh.
        private static readonly TimeSpan GuestTokenLifetime = TimeSpan.FromHours(24);

        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;

        public StartGuestSessionCommandHandler(IUserRepository userRepository, IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<LoginResponseDto> Handle(StartGuestSessionCommand request, CancellationToken cancellationToken)
        {
            var guest = User.CreateGuest(request.Email, request.Phone);
            await _userRepository.SaveAsync(guest);

            return new LoginResponseDto
            {
                Id = guest.Id,
                Message = "Guest session started.",
                AccessToken = _jwtTokenService.GenerateGuestToken(guest, GuestTokenLifetime),
            };
        }
    }
}
