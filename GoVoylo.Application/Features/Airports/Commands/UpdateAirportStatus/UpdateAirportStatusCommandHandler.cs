using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Airports.Dtos;
using GoVoylo.Application.Features.Airports.Mappers;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Airports.Commands.UpdateAirportStatus
{
    public class UpdateAirportStatusCommandHandler : IRequestHandler<UpdateAirportStatusCommand, AirportDto>
    {
        private readonly IAirportRepository _airportRepository;

        public UpdateAirportStatusCommandHandler(IAirportRepository airportRepository)
        {
            _airportRepository = airportRepository;
        }

        public async Task<AirportDto> Handle(
            UpdateAirportStatusCommand request, CancellationToken cancellationToken)
        {
            var airport = await _airportRepository.GetByIataAsync(request.IataCode);

            if (airport == null)
            {
                throw new NotFoundException($"No airport found for IATA code '{request.IataCode}'.");
            }

            airport.SetActive(request.IsActive);
            await _airportRepository.UpdateAsync(airport);

            return AirportMapper.ToDto(airport);
        }
    }
}
