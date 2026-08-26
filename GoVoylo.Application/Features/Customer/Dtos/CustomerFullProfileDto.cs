namespace GoVoylo.Application.Features.Customer.Dtos
{
    public record CustomerFullProfileDto(
        CustomerProfileDto Profile,
        IReadOnlyList<AddressDto> Addresses,
        GstDetailsDto? Gst,
        PreferencesDto Preferences,
        NotificationPreferencesDto NotificationPreferences);
}
