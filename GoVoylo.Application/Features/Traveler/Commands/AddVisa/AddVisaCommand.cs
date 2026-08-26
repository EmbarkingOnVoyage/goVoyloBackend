using GoVoylo.Application.Features.Traveler.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.AddVisa
{
    public record AddVisaCommand(
        Guid UserId,
        Guid TravelerId,
        string Country,
        string VisaNumber,
        string? VisaType,
        DateTime? IssueDate,
        DateTime ExpiryDate) : IRequest<VisaDto>;
}
