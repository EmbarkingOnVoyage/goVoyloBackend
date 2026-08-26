using GoVoylo.Application.Features.Customer.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.UpdateNotificationPreferences
{
    public record UpdateNotificationPreferencesCommand(
        Guid UserId,
        bool EmailMarketing,
        bool SmsTransactional,
        bool SmsMarketing,
        bool PushEnabled) : IRequest<NotificationPreferencesDto>;
}
