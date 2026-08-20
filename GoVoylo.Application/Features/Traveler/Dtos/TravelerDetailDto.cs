namespace GoVoylo.Application.Features.Traveler.Dtos
{
    public record TravelerDetailDto(
        Guid Id,
        string TravelerType,
        string FirstName,
        string LastName,
        DateTime DateOfBirth,
        string? Gender,
        string? Nationality,
        string? MealPreference,
        string? SeatPreference,
        string? City,
        string? State,
        bool AutoAddTravelInsurance,
        IReadOnlyList<string> SpecialAssistance,
        PassportDto? Passport,
        IReadOnlyList<VisaDto> Visas,
        IReadOnlyList<FrequentFlyerDto> FrequentFlyers,
        IReadOnlyList<EmergencyContactDto> EmergencyContacts);

    public record PassportDto(
        Guid Id,
        string MaskedPassportNumber,
        string IssuingCountry,
        DateTime ExpiryDate);

    public record VisaDto(
        Guid Id,
        string Country,
        string MaskedVisaNumber,
        string? VisaType,
        DateTime? IssueDate,
        DateTime ExpiryDate);

    public record FrequentFlyerDto(
        Guid Id,
        string AirlineCode,
        string MaskedMembershipNumber);

    public record EmergencyContactDto(
        Guid Id,
        string Name,
        string? Relationship,
        string Phone,
        string PhoneCountryCode,
        string? Email);
}
