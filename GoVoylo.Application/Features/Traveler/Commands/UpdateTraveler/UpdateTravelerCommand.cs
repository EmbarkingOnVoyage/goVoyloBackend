using GoVoylo.Application.Features.Traveler.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.UpdateTraveler
{
    public record UpdateTravelerCommand(
        Guid UserId,
        Guid TravelerId,
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
        string? PhoneCountryCode = null) : IRequest<TravelerDto>;
}
