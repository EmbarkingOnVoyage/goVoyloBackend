using GoVoylo.Application.Features.Traveler.Services;
using GoVoylo.Application.Features.Traveler.Dtos;
using GoVoylo.Application.Features.Traveler.Mappers;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Queries.GetTravelers
{
    public class GetTravelersQueryHandler : IRequestHandler<GetTravelersQuery, IReadOnlyList<TravelerDto>>
    {
        private readonly AccountHolderTravelerService _accountHolderTravelers;

        public GetTravelersQueryHandler(AccountHolderTravelerService accountHolderTravelers)
        {
            _accountHolderTravelers = accountHolderTravelers;
        }

        public async Task<IReadOnlyList<TravelerDto>> Handle(
            GetTravelersQuery request, CancellationToken cancellationToken)
        {
            var travelers = await _accountHolderTravelers.GetTravelersIncludingAccountHolderAsync(request.UserId);
            return travelers.Select(TravelerMapper.ToDto).ToList();
        }
    }
}
