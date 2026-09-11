using GoVoylo.Application.Features.Holidays.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Holidays.Queries.GetHolidays
{
    public class GetHolidaysQueryHandler : IRequestHandler<GetHolidaysQuery, HolidaysResponseDto>
    {
        private readonly IHolidayCalendarService _holidayCalendarService;

        public GetHolidaysQueryHandler(IHolidayCalendarService holidayCalendarService)
        {
            _holidayCalendarService = holidayCalendarService;
        }

        public async Task<HolidaysResponseDto> Handle(GetHolidaysQuery request, CancellationToken cancellationToken)
        {
            var holidays = await _holidayCalendarService.GetHolidaysAsync(
                request.CountryCode, request.Year, cancellationToken);

            return new HolidaysResponseDto(holidays);
        }
    }
}
