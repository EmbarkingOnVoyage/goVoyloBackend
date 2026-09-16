using GoVoylo.Application.Common;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Traveler.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.UpdateVisa
{
    public class UpdateVisaCommandHandler : IRequestHandler<UpdateVisaCommand, VisaDto>
    {
        private readonly ISavedTravelerRepository _travelerRepository;
        private readonly ITravelerVisaRepository _visaRepository;
        private readonly IEncryptionService _encryptionService;

        public UpdateVisaCommandHandler(
            ISavedTravelerRepository travelerRepository,
            ITravelerVisaRepository visaRepository,
            IEncryptionService encryptionService)
        {
            _travelerRepository = travelerRepository;
            _visaRepository = visaRepository;
            _encryptionService = encryptionService;
        }

        public async Task<VisaDto> Handle(UpdateVisaCommand request, CancellationToken cancellationToken)
        {
            var traveler = await _travelerRepository.GetByIdAsync(request.TravelerId);

            if (traveler == null || traveler.UserId != request.UserId)
            {
                throw new NotFoundException("Traveler not found.");
            }

            var visa = await _visaRepository.GetByIdAsync(request.VisaId);

            if (visa == null || visa.SavedTravelerId != request.TravelerId)
            {
                throw new NotFoundException("Visa not found.");
            }

            visa.Update(
                _encryptionService.Encrypt(request.VisaNumber),
                request.VisaType,
                request.IssueDate,
                request.ExpiryDate);

            await _visaRepository.UpdateAsync(visa);

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
