using GoVoylo.Application.Features.Traveler.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.UpdatePassport
{
    public record UpdatePassportCommand(
        Guid UserId,
        Guid TravelerId,
        string PassportNumber,
        string IssuingCountry,
        DateTime ExpiryDate) : IRequest<PassportDto>;
}
