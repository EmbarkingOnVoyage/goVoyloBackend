using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;

namespace GoVoylo.Application.Features.Traveler.Services
{
    // Makes sure the signed-in customer appears in their own traveller list. Built
    // from the profile once it has a full name and date of birth (a traveller needs
    // both); an existing traveller with the same name and date of birth — the
    // customer saved as a co-traveller earlier — is reused rather than duplicated.
    public class AccountHolderTravelerService
    {
        private readonly ISavedTravelerRepository _travelerRepository;
        private readonly IUserRepository _userRepository;

        public AccountHolderTravelerService(ISavedTravelerRepository travelerRepository, IUserRepository userRepository)
        {
            _travelerRepository = travelerRepository;
            _userRepository = userRepository;
        }

        public async Task<IReadOnlyList<SavedTraveler>> GetTravelersIncludingAccountHolderAsync(Guid userId)
        {
            var travelers = await _travelerRepository.GetByUserIdAsync(userId);
            if (travelers.Any(t => t.IsAccountHolder))
            {
                return travelers;
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null
                || string.IsNullOrWhiteSpace(user.FirstName)
                || string.IsNullOrWhiteSpace(user.LastName)
                || user.DateOfBirth is not { } dateOfBirth)
            {
                return travelers;
            }

            var match = travelers.FirstOrDefault(t =>
                string.Equals(t.FirstName.Trim(), user.FirstName.Trim(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(t.LastName.Trim(), user.LastName.Trim(), StringComparison.OrdinalIgnoreCase)
                && t.DateOfBirth.Date == dateOfBirth.Date);

            if (match != null)
            {
                match.MarkAsAccountHolder(user.Email);
                await _travelerRepository.UpdateAsync(match);
            }
            else
            {
                var self = new SavedTraveler(
                    userId,
                    TravelerTypeFor(dateOfBirth),
                    user.FirstName.Trim(),
                    user.LastName.Trim(),
                    dateOfBirth,
                    user.Gender,
                    user.Nationality,
                    email: user.Email,
                    phone: user.Phone,
                    phoneCountryCode: user.PhoneCountryCode);
                self.MarkAsAccountHolder(user.Email);
                await _travelerRepository.AddAsync(self);
            }

            return await _travelerRepository.GetByUserIdAsync(userId);
        }

        // The account email is the account holder's primary email, whatever was sent.
        public async Task<string?> AccountEmailAsync(Guid userId) =>
            (await _userRepository.GetByIdAsync(userId))?.Email;

        private static string TravelerTypeFor(DateTime dateOfBirth)
        {
            var ageYears = (DateTime.UtcNow.Date - dateOfBirth.Date).TotalDays / 365.25;
            return ageYears < 2 ? "infant" : ageYears <= 12 ? "child" : "adult";
        }
    }
}
