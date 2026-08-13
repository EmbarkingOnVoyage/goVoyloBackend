using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.UpdatePreferences
{
    public class UpdatePreferencesCommandHandler
        : IRequestHandler<UpdatePreferencesCommand, PreferencesDto>
    {
        private readonly IUserPreferenceRepository _preferenceRepository;

        public UpdatePreferencesCommandHandler(IUserPreferenceRepository preferenceRepository)
        {
            _preferenceRepository = preferenceRepository;
        }

        public async Task<PreferencesDto> Handle(
            UpdatePreferencesCommand request,
            CancellationToken cancellationToken)
        {
            var preference = await _preferenceRepository.GetByUserIdAsync(request.UserId);

            if (preference == null)
            {
                preference = new UserPreference(request.UserId, request.Language, request.Currency);
            }
            else
            {
                preference.Update(request.Language, request.Currency);
            }

            await _preferenceRepository.UpsertAsync(preference);

            return new PreferencesDto(preference.Language, preference.Currency);
        }
    }
}
