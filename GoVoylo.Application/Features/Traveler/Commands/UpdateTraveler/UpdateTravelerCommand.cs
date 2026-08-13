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
        string? Nationality) : IRequest<TravelerDto>;
}
