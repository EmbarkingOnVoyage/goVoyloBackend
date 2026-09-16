using GoVoylo.Application.Common;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Traveler.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.AddPassport
{
    public class AddPassportCommandHandler : IRequestHandler<AddPassportCommand, PassportDto>
    {
        private readonly ISavedTravelerRepository _travelerRepository;
        private readonly ITravelerPassportRepository _passportRepository;
        private readonly IEncryptionService _encryptionService;

        public AddPassportCommandHandler(
            ISavedTravelerRepository travelerRepository,
            ITravelerPassportRepository passportRepository,
            IEncryptionService encryptionService)
        {
            _travelerRepository = travelerRepository;
            _passportRepository = passportRepository;
            _encryptionService = encryptionService;
        }

        public async Task<PassportDto> Handle(AddPassportCommand request, CancellationToken cancellationToken)
        {
            var traveler = await _travelerRepository.GetByIdAsync(request.TravelerId);

            if (traveler == null || traveler.UserId != request.UserId)
            {
                throw new NotFoundException("Traveler not found.");
            }

            var existing = await _passportRepository.GetByTravelerIdAsync(traveler.Id);

            if (existing != null)
            {
                throw new ConflictException(
                    "passport_already_exists", "This traveler already has a passport on file — use update instead.");
            }

            var passport = new TravelerPassport(
                traveler.Id,
                _encryptionService.Encrypt(request.PassportNumber),
                request.IssuingCountry,
                request.ExpiryDate);

            await _passportRepository.AddAsync(passport);

            return new PassportDto(
                passport.Id,
                MaskingHelper.MaskKeepLast4(request.PassportNumber),
                passport.IssuingCountry,
                passport.ExpiryDate);
        }
    }
}
