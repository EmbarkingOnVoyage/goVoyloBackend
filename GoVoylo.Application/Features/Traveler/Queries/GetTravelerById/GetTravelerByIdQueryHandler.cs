using GoVoylo.Application.Common;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Traveler.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Queries.GetTravelerById
{
    public class GetTravelerByIdQueryHandler : IRequestHandler<GetTravelerByIdQuery, TravelerDetailDto>
    {
        private readonly ISavedTravelerRepository _travelerRepository;
        private readonly ITravelerPassportRepository _passportRepository;
        private readonly ITravelerVisaRepository _visaRepository;
        private readonly ITravelerFrequentFlyerRepository _frequentFlyerRepository;
        private readonly ITravelerSpecialAssistanceRepository _specialAssistanceRepository;
        private readonly ITravelerEmergencyContactRepository _emergencyContactRepository;
        private readonly IEncryptionService _encryptionService;

        public GetTravelerByIdQueryHandler(
            ISavedTravelerRepository travelerRepository,
            ITravelerPassportRepository passportRepository,
            ITravelerVisaRepository visaRepository,
            ITravelerFrequentFlyerRepository frequentFlyerRepository,
            ITravelerSpecialAssistanceRepository specialAssistanceRepository,
            ITravelerEmergencyContactRepository emergencyContactRepository,
            IEncryptionService encryptionService)
        {
            _travelerRepository = travelerRepository;
            _passportRepository = passportRepository;
            _visaRepository = visaRepository;
            _frequentFlyerRepository = frequentFlyerRepository;
            _specialAssistanceRepository = specialAssistanceRepository;
            _emergencyContactRepository = emergencyContactRepository;
            _encryptionService = encryptionService;
        }

        public async Task<TravelerDetailDto> Handle(
            GetTravelerByIdQuery request, CancellationToken cancellationToken)
        {
            var traveler = await _travelerRepository.GetByIdAsync(request.TravelerId);

            if (traveler == null || traveler.UserId != request.UserId)
            {
                throw new NotFoundException("Traveler not found.");
            }

            var passport = await _passportRepository.GetByTravelerIdAsync(traveler.Id);
            var visas = await _visaRepository.GetByTravelerIdAsync(traveler.Id);
            var frequentFlyers = await _frequentFlyerRepository.GetByTravelerIdAsync(traveler.Id);
            var specialAssistance = await _specialAssistanceRepository.GetByTravelerIdAsync(traveler.Id);
            var emergencyContacts = await _emergencyContactRepository.GetByTravelerIdAsync(traveler.Id);

            var passportDto = passport == null
                ? null
                : new PassportDto(
                    passport.Id,
                    MaskingHelper.MaskKeepLast4(_encryptionService.Decrypt(passport.PassportNumberEncrypted)),
                    passport.IssuingCountry,
                    passport.ExpiryDate);

            return new TravelerDetailDto(
                traveler.Id,
                traveler.TravelerType,
                traveler.FirstName,
                traveler.LastName,
                traveler.DateOfBirth,
                traveler.Gender,
                traveler.Nationality,
                traveler.MealPreference,
                traveler.SeatPreference,
                traveler.City,
                traveler.State,
                traveler.AutoAddTravelInsurance,
                specialAssistance.Select(x => x.SsrCode).ToList(),
                passportDto,
                visas.Select(v => new VisaDto(
                    v.Id,
                    v.Country,
                    MaskingHelper.MaskKeepLast4(_encryptionService.Decrypt(v.VisaNumberEncrypted)),
                    v.VisaType,
                    v.IssueDate,
                    v.ExpiryDate)).ToList(),
                frequentFlyers.Select(f => new FrequentFlyerDto(
                    f.Id,
                    f.AirlineCode,
                    MaskingHelper.MaskKeepLast4(_encryptionService.Decrypt(f.MembershipNumberEncrypted)))).ToList(),
                emergencyContacts.Select(e => new EmergencyContactDto(
                    e.Id, e.Name, e.Relationship, e.Phone, e.PhoneCountryCode, e.Email)).ToList());
        }
    }
}
