using GoVoylo.Application.Features.Traveler.Services;
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
        private readonly AccountHolderTravelerService _accountHolderTravelers;

        public AddTravelerCommandHandler(
            ISavedTravelerRepository travelerRepository,
            AccountHolderTravelerService accountHolderTravelers)
        {
            _travelerRepository = travelerRepository;
            _accountHolderTravelers = accountHolderTravelers;
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

            if (request.IsAccountHolder)
            {
                var existing = await _travelerRepository.GetByUserIdAsync(request.UserId);
                if (existing.Any(t => t.IsAccountHolder))
                {
                    throw new ConflictException(
                        "account_holder_exists",
                        "You're already in your traveller list.");
                }

                // Saved earlier as a co-traveller: make that entry "you" instead of a duplicate.
                var sameIdentity = existing.FirstOrDefault(t =>
                    string.Equals(t.FirstName.Trim(), request.FirstName.Trim(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(t.LastName.Trim(), request.LastName.Trim(), StringComparison.OrdinalIgnoreCase)
                    && t.DateOfBirth.Date == request.DateOfBirth.Date);
                if (sameIdentity != null)
                {
                    sameIdentity.MarkAsAccountHolder(await _accountHolderTravelers.AccountEmailAsync(request.UserId));
                    await _travelerRepository.UpdateAsync(sameIdentity);
                    return TravelerMapper.ToDto(sameIdentity);
                }
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
                request.Nationality,
                request.City,
                request.State,
                request.AutoAddTravelInsurance,
                request.Email,
                request.Phone,
                request.PhoneCountryCode);

            if (request.IsAccountHolder)
            {
                traveler.MarkAsAccountHolder(await _accountHolderTravelers.AccountEmailAsync(request.UserId));
            }

            await _travelerRepository.AddAsync(traveler);

            return TravelerMapper.ToDto(traveler);
        }
    }
}
