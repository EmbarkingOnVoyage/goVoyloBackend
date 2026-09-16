using GoVoylo.Application.Features.Traveler.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.AddPassport
{
    public record AddPassportCommand(
        Guid UserId,
        Guid TravelerId,
        string PassportNumber,
        string IssuingCountry,
        DateTime ExpiryDate) : IRequest<PassportDto>;
}
