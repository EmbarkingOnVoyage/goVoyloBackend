using GoVoylo.Application.Features.Customer.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.UpdateGstDetails
{
    public record UpdateGstDetailsCommand(
        Guid UserId,
        string Gstin,
        string LegalName,
        string? TradeName) : IRequest<GstDetailsDto>;
}
