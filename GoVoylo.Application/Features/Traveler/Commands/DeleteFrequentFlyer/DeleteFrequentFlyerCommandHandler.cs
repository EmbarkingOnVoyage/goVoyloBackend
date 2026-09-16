using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.DeleteFrequentFlyer
{
    public class DeleteFrequentFlyerCommandHandler : IRequestHandler<DeleteFrequentFlyerCommand, Unit>
    {
        private readonly ISavedTravelerRepository _travelerRepository;
        private readonly ITravelerFrequentFlyerRepository _frequentFlyerRepository;

        public DeleteFrequentFlyerCommandHandler(
            ISavedTravelerRepository travelerRepository,
            ITravelerFrequentFlyerRepository frequentFlyerRepository)
        {
            _travelerRepository = travelerRepository;
            _frequentFlyerRepository = frequentFlyerRepository;
        }

        public async Task<Unit> Handle(DeleteFrequentFlyerCommand request, CancellationToken cancellationToken)
        {
            var traveler = await _travelerRepository.GetByIdAsync(request.TravelerId);

            if (traveler == null || traveler.UserId != request.UserId)
            {
                throw new NotFoundException("Traveler not found.");
            }

            var frequentFlyer = await _frequentFlyerRepository.GetByIdAsync(request.FrequentFlyerId);

            if (frequentFlyer == null || frequentFlyer.SavedTravelerId != request.TravelerId)
            {
                throw new NotFoundException("Frequent flyer membership not found.");
            }

            await _frequentFlyerRepository.DeleteAsync(frequentFlyer);
            return Unit.Value;
        }
    }
}
