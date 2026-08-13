using GoVoylo.Application.Features.Traveler.Dtos;
using GoVoylo.Domain.Entities;

namespace GoVoylo.Application.Features.Traveler.Mappers
{
    public static class TravelerMapper
    {
        public static TravelerDto ToDto(SavedTraveler traveler)
        {
            return new TravelerDto(
                traveler.Id,
                traveler.TravelerType,
                traveler.FirstName,
                traveler.LastName,
                traveler.DateOfBirth,
                traveler.Gender,
                traveler.Nationality);
        }
    }
}
