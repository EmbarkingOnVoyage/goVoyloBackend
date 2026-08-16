using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Traveler.Dtos;
using GoVoylo.Application.Features.Traveler.Mappers;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.AddTraveler
{
    public class AddTravelerCommandHandler : IRequestHandler<AddTravelerCommand, TravelerDto>
    {
        private const int MaxTravelersPerCustomer = 50;

        private readonly ISavedTravelerRepository _travelerRepository;

        public AddTravelerCommandHandler(ISavedTravelerRepository travelerRepository)
        {
            _travelerRepository = travelerRepository;
        }

        public async Task<TravelerDto> Handle(AddTravelerCommand request, CancellationToken cancellationToken)
        {
            var existingCount = await _travelerRepository.CountByUserIdAsync(request.UserId);

            if (existingCount >= MaxTravelersPerCustomer)
            {
                throw new BusinessRuleException(
                    "max_travelers_reached",
                    $"You can save up to {MaxTravelersPerCustomer} travelers.");
            }

            var isDuplicate = await _travelerRepository.ExistsByIdentityAsync(
                request.UserId, request.FirstName, request.LastName, request.DateOfBirth);

            if (isDuplicate)
            {
                throw new ConflictException(
                    "traveler_already_exists",
                    "A traveler with this name and date of birth is already saved.");
            }

            var traveler = new SavedTraveler(
                request.UserId,
                request.TravelerType.ToLowerInvariant(),
                request.FirstName,
                request.LastName,
                request.DateOfBirth,
                request.Gender,
                request.Nationality);

            await _travelerRepository.AddAsync(traveler);

            return TravelerMapper.ToDto(traveler);
        }
    }
}
