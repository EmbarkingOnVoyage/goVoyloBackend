using GoVoylo.Application.Features.Customer.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.AddGstDetails
{
    public record AddGstDetailsCommand(
        Guid UserId,
        string Gstin,
        string LegalName,
        string? TradeName) : IRequest<GstDetailsDto>;
}
