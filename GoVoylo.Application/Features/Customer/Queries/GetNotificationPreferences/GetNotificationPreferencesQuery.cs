using GoVoylo.Application.Features.Customer.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Queries.GetNotificationPreferences
{
    public record GetNotificationPreferencesQuery(Guid UserId) : IRequest<NotificationPreferencesDto>;
}
