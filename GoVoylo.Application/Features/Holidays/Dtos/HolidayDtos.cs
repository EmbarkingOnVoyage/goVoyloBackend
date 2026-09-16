namespace GoVoylo.Application.Features.Holidays.Dtos
{
    public record HolidayDto(DateTime Date, string Name);

    public record HolidaysResponseDto(IReadOnlyList<HolidayDto> Holidays);
}
