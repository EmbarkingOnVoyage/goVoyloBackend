using GoVoylo.Application.Features.Pricing.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Pricing.Queries.GetConvenienceFeeOffer
{
    // Whether the signed-in customer's next booking is free of the convenience fee
    // (see IConvenienceFeeService.GetOfferAsync) — the app shows it before booking.
    public record GetConvenienceFeeOfferQuery(Guid UserId) : IRequest<ConvenienceFeeOfferDto>;

    public class GetConvenienceFeeOfferQueryHandler : IRequestHandler<GetConvenienceFeeOfferQuery, ConvenienceFeeOfferDto>
    {
        private readonly IConvenienceFeeService _convenienceFeeService;

        public GetConvenienceFeeOfferQueryHandler(IConvenienceFeeService convenienceFeeService)
        {
            _convenienceFeeService = convenienceFeeService;
        }

        public Task<ConvenienceFeeOfferDto> Handle(GetConvenienceFeeOfferQuery request, CancellationToken cancellationToken) =>
            _convenienceFeeService.GetOfferAsync(request.UserId, cancellationToken);
    }
}
