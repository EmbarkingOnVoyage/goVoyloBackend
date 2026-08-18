using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.RepriceFlightOffer
{
    public record RepriceFlightOfferQuery(Guid OfferId) : IRequest<FlightRepriceResponseDto>;
}
