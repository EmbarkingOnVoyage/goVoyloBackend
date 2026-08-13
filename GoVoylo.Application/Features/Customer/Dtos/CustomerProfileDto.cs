namespace GoVoylo.Application.Features.Customer.Dtos
{
    public record CustomerProfileDto(
        Guid Id,
        string FirstName,
        string LastName,
        string? Email,
        string? Phone,
        bool IsEmailVerified,
        bool IsPhoneVerified,
        string? ProfileImageUrl,
        string Status,
        int ProfileCompletionPercentage,
        DateTime CreatedAt);
}
