using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.UpdateTravelerPreferences
{
    public record UpdateTravelerPreferencesCommand(
        Guid UserId,
        Guid TravelerId,
        string? MealPreference,
        string? SeatPreference,
        IReadOnlyList<string> SpecialAssistance) : IRequest<Unit>;
}
