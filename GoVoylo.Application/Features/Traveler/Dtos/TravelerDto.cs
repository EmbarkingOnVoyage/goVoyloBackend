namespace GoVoylo.Application.Features.Traveler.Dtos
{
    public record TravelerDto(
        Guid Id,
        string TravelerType,
        string FirstName,
        string LastName,
        DateTime DateOfBirth,
        string? Gender,
        string? Nationality);
}
