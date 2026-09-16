using GoVoylo.Application.Features.Customer.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.UpdateExtendedProfile
{
    public record UpdateExtendedProfileCommand(
        Guid UserId,
        string? Gender,
        DateTime? DateOfBirth,
        string? Nationality,
        string? MaritalStatus,
        DateTime? Anniversary,
        string? CityOfResidence,
        string? State,
        string? PassportNumber,
        DateTime? PassportExpiryDate,
        string? PassportIssuingCountry,
        string? PanCardNumber,
        bool AutoAddTravelInsurance) : IRequest<CustomerProfileDto>;
}
