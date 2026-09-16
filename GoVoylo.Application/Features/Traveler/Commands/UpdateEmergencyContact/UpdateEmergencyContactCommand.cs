using GoVoylo.Application.Features.Traveler.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.UpdateEmergencyContact
{
    public record UpdateEmergencyContactCommand(
        Guid UserId,
        Guid TravelerId,
        Guid ContactId,
        string Name,
        string? Relationship,
        string Phone,
        string PhoneCountryCode,
        string? Email) : IRequest<EmergencyContactDto>;
}
