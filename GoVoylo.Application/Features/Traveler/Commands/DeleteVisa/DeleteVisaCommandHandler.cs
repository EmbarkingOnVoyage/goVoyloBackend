using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.DeleteVisa
{
    public class DeleteVisaCommandHandler : IRequestHandler<DeleteVisaCommand, Unit>
    {
        private readonly ISavedTravelerRepository _travelerRepository;
        private readonly ITravelerVisaRepository _visaRepository;

        public DeleteVisaCommandHandler(
            ISavedTravelerRepository travelerRepository,
            ITravelerVisaRepository visaRepository)
        {
            _travelerRepository = travelerRepository;
            _visaRepository = visaRepository;
        }

        public async Task<Unit> Handle(DeleteVisaCommand request, CancellationToken cancellationToken)
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

            await _visaRepository.DeleteAsync(visa);
            return Unit.Value;
        }
    }
}
