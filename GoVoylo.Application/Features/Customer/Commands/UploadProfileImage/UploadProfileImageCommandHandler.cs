using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.UploadProfileImage
{
    public class UploadProfileImageCommandHandler
        : IRequestHandler<UploadProfileImageCommand, string>
    {
        private readonly IUserRepository _userRepository;
        private readonly IProfileImageStorageService _storageService;

        public UploadProfileImageCommandHandler(
            IUserRepository userRepository,
            IProfileImageStorageService storageService)
        {
            _userRepository = userRepository;
            _storageService = storageService;
        }

        public async Task<string> Handle(
            UploadProfileImageCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                throw new NotFoundException("Customer profile not found.");
            }

            var imageUrl = await _storageService.UploadAsync(
                request.UserId,
                request.FileStream,
                request.FileName,
                request.ContentType,
                cancellationToken);

            user.SetProfileImageUrl(imageUrl);
            await _userRepository.UpdateAsync(user);

            return imageUrl;
        }
    }
}
