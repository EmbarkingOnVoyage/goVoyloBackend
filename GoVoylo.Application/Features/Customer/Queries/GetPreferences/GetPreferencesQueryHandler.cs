using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Queries.GetPreferences
{
    public class GetPreferencesQueryHandler
        : IRequestHandler<GetPreferencesQuery, PreferencesDto>
    {
        private readonly IUserPreferenceRepository _preferenceRepository;

        public GetPreferencesQueryHandler(IUserPreferenceRepository preferenceRepository)
        {
            _preferenceRepository = preferenceRepository;
        }

        public async Task<PreferencesDto> Handle(
            GetPreferencesQuery request,
            CancellationToken cancellationToken)
        {
            var preference = await _preferenceRepository.GetByUserIdAsync(request.UserId);

            // No row yet for a new customer — regional defaults, not a 404 (ticket GV-CUST-BE-007).
            return preference == null
                ? new PreferencesDto("en", "INR")
                : new PreferencesDto(preference.Language, preference.Currency);
        }
    }
}
