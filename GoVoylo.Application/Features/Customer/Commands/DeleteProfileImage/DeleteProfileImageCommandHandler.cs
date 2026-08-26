using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.DeleteProfileImage
{
    public class DeleteProfileImageCommandHandler
        : IRequestHandler<DeleteProfileImageCommand, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly IProfileImageStorageService _storageService;

        public DeleteProfileImageCommandHandler(
            IUserRepository userRepository,
            IProfileImageStorageService storageService)
        {
            _userRepository = userRepository;
            _storageService = storageService;
        }

        public async Task<Unit> Handle(
            DeleteProfileImageCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                throw new NotFoundException("Customer profile not found.");
            }

            if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
            {
                await _storageService.DeleteAsync(user.ProfileImageUrl, cancellationToken);
                user.ClearProfileImageUrl();
                await _userRepository.UpdateAsync(user);
            }

            return Unit.Value;
        }
    }
}
