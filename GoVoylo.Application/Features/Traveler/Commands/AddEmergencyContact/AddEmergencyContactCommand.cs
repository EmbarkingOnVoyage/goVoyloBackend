using GoVoylo.Application.Features.Traveler.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.AddEmergencyContact
{
    public record AddEmergencyContactCommand(
        Guid UserId,
        Guid TravelerId,
        string Name,
        string? Relationship,
        string Phone,
        string PhoneCountryCode,
        string? Email) : IRequest<EmergencyContactDto>;
}
