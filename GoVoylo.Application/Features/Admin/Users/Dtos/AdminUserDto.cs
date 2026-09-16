namespace GoVoylo.Application.Features.Admin.Users.Dtos
{
    public record AdminUserDto(
        Guid Id,
        string FirstName,
        string LastName,
        string? Email,
        string? Phone,
        string Status,
        IReadOnlyList<string> Roles,
        DateTime CreatedAt);
}
