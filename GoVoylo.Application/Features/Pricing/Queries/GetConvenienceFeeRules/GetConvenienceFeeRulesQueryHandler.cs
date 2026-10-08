using GoVoylo.Application.Features.Pricing.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Pricing.Queries.GetConvenienceFeeRules
{
    public class GetConvenienceFeeRulesQueryHandler : IRequestHandler<GetConvenienceFeeRulesQuery, ConvenienceFeeRulesDto>
    {
        private readonly IConvenienceFeeService _convenienceFeeService;

        public GetConvenienceFeeRulesQueryHandler(IConvenienceFeeService convenienceFeeService)
        {
            _convenienceFeeService = convenienceFeeService;
        }

        public Task<ConvenienceFeeRulesDto> Handle(GetConvenienceFeeRulesQuery request, CancellationToken cancellationToken) =>
            _convenienceFeeService.GetRulesAsync(cancellationToken);
    }
}
