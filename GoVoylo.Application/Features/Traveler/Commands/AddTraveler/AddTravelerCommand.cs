using GoVoylo.Application.Features.Traveler.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.AddTraveler
{
    public record AddTravelerCommand(
        Guid UserId,
        string TravelerType,
        string FirstName,
        string LastName,
        DateTime DateOfBirth,
        string? Gender,
        string? Nationality,
        string? City,
        string? State,
        bool AutoAddTravelInsurance,
        string? Email = null,
        string? Phone = null,
        string? PhoneCountryCode = null,
        // Saving the customer themselves ("Add yourself" when the profile was too
        // incomplete to create them automatically).
        bool IsAccountHolder = false) : IRequest<TravelerDto>;
}
