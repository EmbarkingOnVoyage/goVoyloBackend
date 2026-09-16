using GoVoylo.Application.Features.Customer.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.UpdatePreferences
{
    public record UpdatePreferencesCommand(
        Guid UserId,
        string Language,
        string Currency) : IRequest<PreferencesDto>;
}
