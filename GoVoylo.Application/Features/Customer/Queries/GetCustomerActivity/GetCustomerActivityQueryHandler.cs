using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Queries.GetCustomerActivity
{
    public class GetCustomerActivityQueryHandler
        : IRequestHandler<GetCustomerActivityQuery, IReadOnlyList<ActivityLogDto>>
    {
        private const int MaxResults = 50;

        private readonly IActivityLogRepository _activityLogRepository;

        public GetCustomerActivityQueryHandler(IActivityLogRepository activityLogRepository)
        {
            _activityLogRepository = activityLogRepository;
        }

        public async Task<IReadOnlyList<ActivityLogDto>> Handle(
            GetCustomerActivityQuery request,
            CancellationToken cancellationToken)
        {
            var logs = await _activityLogRepository.GetByUserIdAsync(
                request.UserId.ToString(), MaxResults, cancellationToken);

            return logs
                .Select(x => new ActivityLogDto(x.ActionType, x.PayloadJson, x.SourcePlatform, x.CreatedAt))
                .ToList();
        }
    }
}
