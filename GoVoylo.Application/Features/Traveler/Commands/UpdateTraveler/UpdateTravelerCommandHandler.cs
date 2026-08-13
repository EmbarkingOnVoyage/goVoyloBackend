using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Traveler.Dtos;
using GoVoylo.Application.Features.Traveler.Mappers;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.UpdateTraveler
{
    public class UpdateTravelerCommandHandler : IRequestHandler<UpdateTravelerCommand, TravelerDto>
    {
        private readonly ISavedTravelerRepository _travelerRepository;

        public UpdateTravelerCommandHandler(ISavedTravelerRepository travelerRepository)
        {
            _travelerRepository = travelerRepository;
        }

        public async Task<TravelerDto> Handle(UpdateTravelerCommand request, CancellationToken cancellationToken)
        {
            var traveler = await _travelerRepository.GetByIdAsync(request.TravelerId);

            if (traveler == null || traveler.UserId != request.UserId)
            {
                throw new NotFoundException("Traveler not found.");
            }

            traveler.Update(
                request.TravelerType.ToLowerInvariant(),
                request.FirstName,
                request.LastName,
                request.DateOfBirth,
                request.Gender,
                request.Nationality);

            await _travelerRepository.UpdateAsync(traveler);

            return TravelerMapper.ToDto(traveler);
        }
    }
}
