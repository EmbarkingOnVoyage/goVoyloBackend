using GoVoylo.Application.Common;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Traveler.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.AddFrequentFlyer
{
    public class AddFrequentFlyerCommandHandler : IRequestHandler<AddFrequentFlyerCommand, FrequentFlyerDto>
    {
        private readonly ISavedTravelerRepository _travelerRepository;
        private readonly ITravelerFrequentFlyerRepository _frequentFlyerRepository;
        private readonly IEncryptionService _encryptionService;

        public AddFrequentFlyerCommandHandler(
            ISavedTravelerRepository travelerRepository,
            ITravelerFrequentFlyerRepository frequentFlyerRepository,
            IEncryptionService encryptionService)
        {
            _travelerRepository = travelerRepository;
            _frequentFlyerRepository = frequentFlyerRepository;
            _encryptionService = encryptionService;
        }

        public async Task<FrequentFlyerDto> Handle(
            AddFrequentFlyerCommand request, CancellationToken cancellationToken)
        {
            var traveler = await _travelerRepository.GetByIdAsync(request.TravelerId);

            if (traveler == null || traveler.UserId != request.UserId)
            {
                throw new NotFoundException("Traveler not found.");
            }

            var airlineCode = request.AirlineCode.ToUpperInvariant();

            if (await _frequentFlyerRepository.ExistsForAirlineAsync(traveler.Id, airlineCode))
            {
                throw new ConflictException(
                    "frequent_flyer_already_exists",
                    "This traveler already has a frequent flyer membership for this airline.");
            }

            var frequentFlyer = new TravelerFrequentFlyer(
                traveler.Id, airlineCode, _encryptionService.Encrypt(request.MembershipNumber));

            await _frequentFlyerRepository.AddAsync(frequentFlyer);

            return new FrequentFlyerDto(
                frequentFlyer.Id,
                frequentFlyer.AirlineCode,
                MaskingHelper.MaskKeepLast4(request.MembershipNumber));
        }
    }
}
