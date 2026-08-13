using GoVoylo.Application.Features.Customer.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Queries.GetGstDetails
{
    public record GetGstDetailsQuery(Guid UserId) : IRequest<GstDetailsDto?>;
}
