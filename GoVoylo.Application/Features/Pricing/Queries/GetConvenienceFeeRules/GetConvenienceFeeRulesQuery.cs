using GoVoylo.Application.Features.Pricing.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Pricing.Queries.GetConvenienceFeeRules
{
    public record GetConvenienceFeeRulesQuery : IRequest<ConvenienceFeeRulesDto>;
}
