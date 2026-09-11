using GoVoylo.Application.Features.Holidays.Dtos;

namespace GoVoylo.Application.Interfaces
{
    public interface IHolidayCalendarService
    {
        // countryCode is an ISO-3166 alpha-2 code ("IN", "US", ...) — kept as a
        // parameter (not hardcoded to India) so switching to the user's actual
        // country later, once geo-location is wired up, is just a different
        // argument, not new plumbing.
        Task<IReadOnlyList<HolidayDto>> GetHolidaysAsync(
            string countryCode, int year, CancellationToken cancellationToken);
    }
}
