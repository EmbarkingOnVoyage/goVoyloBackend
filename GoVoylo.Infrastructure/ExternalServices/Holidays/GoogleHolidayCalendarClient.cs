using System.Globalization;
using System.Text.RegularExpressions;
using GoVoylo.Application.Features.Holidays.Dtos;
using GoVoylo.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace GoVoylo.Infrastructure.ExternalServices.Holidays
{
    // Reads Google's public "Holidays in <country>" calendars — no API key, no
    // signup, and Google keeps them populated several years ahead and corrects
    // moon-sighting-dependent dates (Eid, etc.) as they're confirmed. This
    // replaces what used to be a hand-curated, single-year holiday list that
    // needed manual updates every year.
    public class GoogleHolidayCalendarClient : IHolidayCalendarService
    {
        private static readonly Dictionary<string, string> CalendarIdsByCountry = new(StringComparer.OrdinalIgnoreCase)
        {
            ["IN"] = "en.indian#holiday@group.v.calendar.google.com",
        };

        // The ICS feed covers many years in one response, so the whole parsed
        // calendar is cached per country rather than re-fetching per year —
        // one HTTP call serves every year that gets asked for.
        private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(7);

        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public GoogleHolidayCalendarClient(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<IReadOnlyList<HolidayDto>> GetHolidaysAsync(
            string countryCode, int year, CancellationToken cancellationToken)
        {
            var allHolidays = await _cache.GetOrCreateAsync(
                $"holiday-calendar:{countryCode.ToUpperInvariant()}",
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                    return await FetchAndParseAsync(countryCode, cancellationToken);
                });

            return (allHolidays ?? new List<HolidayDto>())
                .Where(h => h.Date.Year == year)
                .OrderBy(h => h.Date)
                .ToList();
        }

        private async Task<List<HolidayDto>> FetchAndParseAsync(string countryCode, CancellationToken cancellationToken)
        {
            if (!CalendarIdsByCountry.TryGetValue(countryCode, out var calendarId))
            {
                // No calendar mapped for this country yet — return nothing
                // rather than fail the whole search-form page over a missing
                // holiday overlay.
                return new List<HolidayDto>();
            }

            var url = $"https://calendar.google.com/calendar/ical/{Uri.EscapeDataString(calendarId)}/public/basic.ics";
            var ics = await _httpClient.GetStringAsync(url, cancellationToken);

            return ParseIcs(ics);
        }

        // Minimal parser for just the fields this needs (DTSTART;VALUE=DATE,
        // SUMMARY, DESCRIPTION) rather than a full RFC 5545 implementation.
        // DESCRIPTION only needs its first physical line: Google's "Public
        // holiday" value is always short enough to never fold across lines,
        // while "Observance" always does (it's followed by an instructional
        // sentence) — so checking the first line is enough to tell them apart
        // without needing to un-fold continuation lines.
        private static List<HolidayDto> ParseIcs(string ics)
        {
            var holidays = new List<HolidayDto>();

            foreach (var block in ics.Split("BEGIN:VEVENT", StringSplitOptions.RemoveEmptyEntries).Skip(1))
            {
                var dateMatch = Regex.Match(block, @"DTSTART;VALUE=DATE:(\d{8})");
                var summaryMatch = Regex.Match(block, @"SUMMARY:(.+)");
                var descriptionMatch = Regex.Match(block, @"DESCRIPTION:(.+)");

                if (!dateMatch.Success || !summaryMatch.Success) continue;
                if (!descriptionMatch.Success || !descriptionMatch.Groups[1].Value.TrimStart().StartsWith("Public holiday"))
                {
                    continue;
                }

                if (!DateTime.TryParseExact(
                        dateMatch.Groups[1].Value, "yyyyMMdd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var date))
                {
                    continue;
                }

                holidays.Add(new HolidayDto(date, summaryMatch.Groups[1].Value.Trim()));
            }

            return holidays;
        }
    }
}
