using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GoVoylo.Application.Features.Authentication.Services
{
    // On sign-in, claims every guest checkout made with the account's email: the
    // guest's travellers and bookings move into the account, so they show up in
    // its travellers and My Trips. The email OTP has just proved the address is
    // theirs, which is what makes handing this data over safe.
    public class GuestAccountMerger
    {
        private readonly IUserRepository _userRepository;
        private readonly ISavedTravelerRepository _travelerRepository;
        private readonly ITripBookingRepository _tripBookingRepository;
        private readonly ILogger<GuestAccountMerger> _logger;

        public GuestAccountMerger(
            IUserRepository userRepository,
            ISavedTravelerRepository travelerRepository,
            ITripBookingRepository tripBookingRepository,
            ILogger<GuestAccountMerger> logger)
        {
            _userRepository = userRepository;
            _travelerRepository = travelerRepository;
            _tripBookingRepository = tripBookingRepository;
            _logger = logger;
        }

        public async Task MergeIntoAsync(User account, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(account.Email))
            {
                return;
            }

            var guests = await _userRepository.GetGuestsByEmailAsync(account.Email);
            if (guests.Count == 0)
            {
                return;
            }

            var travelers = (await _travelerRepository.GetByUserIdAsync(account.Id)).ToList();

            foreach (var guest in guests)
            {
                // Best-effort per guest: a failure must never block signing in;
                // the guest stays unmerged and is picked up on the next sign-in.
                try
                {
                    foreach (var traveler in await _travelerRepository.GetByUserIdAsync(guest.Id))
                    {
                        // Someone already saved on the account isn't added twice.
                        if (travelers.Any(t => SameIdentity(t, traveler)))
                        {
                            traveler.SoftDelete();
                        }
                        else
                        {
                            traveler.MoveToUser(account.Id);
                            travelers.Add(traveler);
                        }
                        await _travelerRepository.UpdateAsync(traveler);
                    }

                    foreach (var booking in await _tripBookingRepository.GetByUserIdAsync(guest.Id, cancellationToken))
                    {
                        booking.MoveToUser(account.Id);
                        await _tripBookingRepository.UpdateAsync(booking, cancellationToken);
                    }

                    // The mobile given at checkout becomes the account's, if it has none.
                    if (string.IsNullOrWhiteSpace(account.Phone) && !string.IsNullOrWhiteSpace(guest.Phone))
                    {
                        account.SetContactDetails(guest.Phone, null);
                        await _userRepository.UpdateAsync(account);
                    }

                    guest.MarkGuestMerged();
                    await _userRepository.UpdateAsync(guest);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Merging guest {GuestId} into user {UserId} failed.", guest.Id, account.Id);
                }
            }
        }

        private static bool SameIdentity(SavedTraveler a, SavedTraveler b) =>
            string.Equals(a.FirstName.Trim(), b.FirstName.Trim(), StringComparison.OrdinalIgnoreCase)
            && string.Equals(a.LastName.Trim(), b.LastName.Trim(), StringComparison.OrdinalIgnoreCase)
            && a.DateOfBirth.Date == b.DateOfBirth.Date;
    }
}
