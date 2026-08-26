using GoVoylo.Application.Features.Traveler.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.UpdateVisa
{
    public record UpdateVisaCommand(
        Guid UserId,
        Guid TravelerId,
        Guid VisaId,
        string VisaNumber,
        string? VisaType,
        DateTime? IssueDate,
        DateTime ExpiryDate) : IRequest<VisaDto>;
}
