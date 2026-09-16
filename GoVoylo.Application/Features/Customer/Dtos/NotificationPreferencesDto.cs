namespace GoVoylo.Application.Features.Customer.Dtos
{
    public record NotificationPreferencesDto(
        bool EmailTransactional,
        bool EmailMarketing,
        bool SmsTransactional,
        bool SmsMarketing,
        bool PushEnabled);
}
