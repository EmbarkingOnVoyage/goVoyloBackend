using GoVoylo.Application.Common;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Traveler.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.UpdatePassport
{
    public class UpdatePassportCommandHandler : IRequestHandler<UpdatePassportCommand, PassportDto>
    {
        private readonly ISavedTravelerRepository _travelerRepository;
        private readonly ITravelerPassportRepository _passportRepository;
        private readonly IEncryptionService _encryptionService;

        public UpdatePassportCommandHandler(
            ISavedTravelerRepository travelerRepository,
            ITravelerPassportRepository passportRepository,
            IEncryptionService encryptionService)
        {
            _travelerRepository = travelerRepository;
            _passportRepository = passportRepository;
            _encryptionService = encryptionService;
        }

        public async Task<PassportDto> Handle(UpdatePassportCommand request, CancellationToken cancellationToken)
        {
            var traveler = await _travelerRepository.GetByIdAsync(request.TravelerId);

            if (traveler == null || traveler.UserId != request.UserId)
            {
                throw new NotFoundException("Traveler not found.");
            }

            var passport = await _passportRepository.GetByTravelerIdAsync(traveler.Id);

            if (passport == null)
            {
                throw new NotFoundException("No passport on file for this traveler — add one first.");
            }

            passport.Update(
                _encryptionService.Encrypt(request.PassportNumber),
                request.IssuingCountry,
                request.ExpiryDate);

            await _passportRepository.UpdateAsync(passport);

            return new PassportDto(
                passport.Id,
                MaskingHelper.MaskKeepLast4(request.PassportNumber),
                passport.IssuingCountry,
                passport.ExpiryDate);
        }
    }
}
