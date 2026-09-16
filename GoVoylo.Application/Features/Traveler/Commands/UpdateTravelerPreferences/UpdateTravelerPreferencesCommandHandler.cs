using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.UpdateTravelerPreferences
{
    public class UpdateTravelerPreferencesCommandHandler
        : IRequestHandler<UpdateTravelerPreferencesCommand, Unit>
    {
        private readonly ISavedTravelerRepository _travelerRepository;
        private readonly ITravelerSpecialAssistanceRepository _specialAssistanceRepository;

        public UpdateTravelerPreferencesCommandHandler(
            ISavedTravelerRepository travelerRepository,
            ITravelerSpecialAssistanceRepository specialAssistanceRepository)
        {
            _travelerRepository = travelerRepository;
            _specialAssistanceRepository = specialAssistanceRepository;
        }

        public async Task<Unit> Handle(
            UpdateTravelerPreferencesCommand request, CancellationToken cancellationToken)
        {
            var traveler = await _travelerRepository.GetByIdAsync(request.TravelerId);

            if (traveler == null || traveler.UserId != request.UserId)
            {
                throw new NotFoundException("Traveler not found.");
            }

            traveler.UpdatePreferences(request.MealPreference, request.SeatPreference);
            await _travelerRepository.UpdateAsync(traveler);

            var assistanceEntities = request.SpecialAssistance
                .Select(code => new TravelerSpecialAssistance(traveler.Id, code.ToUpperInvariant(), null));

            await _specialAssistanceRepository.ReplaceAllAsync(traveler.Id, assistanceEntities);

            return Unit.Value;
        }
    }
}
