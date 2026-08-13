using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Queries.GetNotificationPreferences
{
    public class GetNotificationPreferencesQueryHandler
        : IRequestHandler<GetNotificationPreferencesQuery, NotificationPreferencesDto>
    {
        private readonly INotificationPreferenceRepository _repository;

        public GetNotificationPreferencesQueryHandler(INotificationPreferenceRepository repository)
        {
            _repository = repository;
        }

        public async Task<NotificationPreferencesDto> Handle(
            GetNotificationPreferencesQuery request,
            CancellationToken cancellationToken)
        {
            var preference = await _repository.GetByUserIdAsync(request.UserId);

            // No row yet — ship the same defaults the entity constructor would set.
            return preference == null
                ? new NotificationPreferencesDto(true, true, true, false, true)
                : new NotificationPreferencesDto(
                    preference.EmailTransactional,
                    preference.EmailMarketing,
                    preference.SmsTransactional,
                    preference.SmsMarketing,
                    preference.PushEnabled);
        }
    }
}
