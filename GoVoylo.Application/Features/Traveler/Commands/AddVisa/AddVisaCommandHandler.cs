using GoVoylo.Application.Common;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Traveler.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.AddVisa
{
    public class AddVisaCommandHandler : IRequestHandler<AddVisaCommand, VisaDto>
    {
        private readonly ISavedTravelerRepository _travelerRepository;
        private readonly ITravelerVisaRepository _visaRepository;
        private readonly IEncryptionService _encryptionService;

        public AddVisaCommandHandler(
            ISavedTravelerRepository travelerRepository,
            ITravelerVisaRepository visaRepository,
            IEncryptionService encryptionService)
        {
            _travelerRepository = travelerRepository;
            _visaRepository = visaRepository;
            _encryptionService = encryptionService;
        }

        public async Task<VisaDto> Handle(AddVisaCommand request, CancellationToken cancellationToken)
        {
            var traveler = await _travelerRepository.GetByIdAsync(request.TravelerId);

            if (traveler == null || traveler.UserId != request.UserId)
            {
                throw new NotFoundException("Traveler not found.");
            }

            if (await _visaRepository.ExistsForCountryAsync(traveler.Id, request.Country))
            {
                throw new ConflictException(
                    "visa_already_exists", "This traveler already has a visa on file for this country.");
            }

            var visa = new TravelerVisa(
                traveler.Id,
                request.Country,
                _encryptionService.Encrypt(request.VisaNumber),
                request.VisaType,
                request.IssueDate,
                request.ExpiryDate);

            await _visaRepository.AddAsync(visa);

            return new VisaDto(
                visa.Id,
                visa.Country,
                MaskingHelper.MaskKeepLast4(request.VisaNumber),
                visa.VisaType,
                visa.IssueDate,
                visa.ExpiryDate);
        }
    }
}
