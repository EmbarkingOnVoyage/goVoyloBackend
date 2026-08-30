using GoVoylo.Application.Features.Flights.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.FilterFlightOffers
{
    public record FilterFlightOffersQuery(FlightOfferFilterRequestDto Filter) : IRequest<FlightSearchResponseDto>;
}
