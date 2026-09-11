using GoVoylo.Application.Features.Holidays.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Holidays.Queries.GetHolidays
{
    public record GetHolidaysQuery(string CountryCode, int Year) : IRequest<HolidaysResponseDto>;
}
