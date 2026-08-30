using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Airports.Dtos;
using GoVoylo.Application.Features.Airports.Mappers;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Airports.Queries.GetAirport
{
    public class GetAirportQueryHandler : IRequestHandler<GetAirportQuery, AirportDto>
    {
        private readonly IAirportRepository _airportRepository;

        public GetAirportQueryHandler(IAirportRepository airportRepository)
        {
            _airportRepository = airportRepository;
        }

        public async Task<AirportDto> Handle(GetAirportQuery request, CancellationToken cancellationToken)
        {
            var airport = await _airportRepository.GetByIataAsync(request.IataCode);

            if (airport == null)
            {
                throw new NotFoundException($"No airport found for IATA code '{request.IataCode}'.");
            }

            return AirportMapper.ToDto(airport);
        }
    }
}
