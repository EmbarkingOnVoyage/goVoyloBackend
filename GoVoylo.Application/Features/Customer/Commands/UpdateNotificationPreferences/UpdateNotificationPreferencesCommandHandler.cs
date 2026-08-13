using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.UpdateNotificationPreferences
{
    public class UpdateNotificationPreferencesCommandHandler
        : IRequestHandler<UpdateNotificationPreferencesCommand, NotificationPreferencesDto>
    {
        private readonly INotificationPreferenceRepository _repository;

        public UpdateNotificationPreferencesCommandHandler(INotificationPreferenceRepository repository)
        {
            _repository = repository;
        }

        public async Task<NotificationPreferencesDto> Handle(
            UpdateNotificationPreferencesCommand request,
            CancellationToken cancellationToken)
        {
            var preference = await _repository.GetByUserIdAsync(request.UserId);

            if (preference == null)
            {
                preference = new NotificationPreference(request.UserId);
            }

            preference.Update(
                request.EmailMarketing,
                request.SmsTransactional,
                request.SmsMarketing,
                request.PushEnabled);

            await _repository.UpsertAsync(preference);

            return new NotificationPreferencesDto(
                preference.EmailTransactional,
                preference.EmailMarketing,
                preference.SmsTransactional,
                preference.SmsMarketing,
                preference.PushEnabled);
        }
    }
}
