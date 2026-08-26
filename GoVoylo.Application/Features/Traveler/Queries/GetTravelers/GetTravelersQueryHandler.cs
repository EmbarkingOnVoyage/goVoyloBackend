using GoVoylo.Application.Features.Traveler.Dtos;
using GoVoylo.Application.Features.Traveler.Mappers;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Queries.GetTravelers
{
    public class GetTravelersQueryHandler : IRequestHandler<GetTravelersQuery, IReadOnlyList<TravelerDto>>
    {
        private readonly ISavedTravelerRepository _travelerRepository;

        public GetTravelersQueryHandler(ISavedTravelerRepository travelerRepository)
        {
            _travelerRepository = travelerRepository;
        }

        public async Task<IReadOnlyList<TravelerDto>> Handle(
            GetTravelersQuery request, CancellationToken cancellationToken)
        {
            var travelers = await _travelerRepository.GetByUserIdAsync(request.UserId);
            return travelers.Select(TravelerMapper.ToDto).ToList();
        }
    }
}
