using GoVoylo.Application.Features.Customer.Dtos;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Queries.GetPreferences
{
    public record GetPreferencesQuery(Guid UserId) : IRequest<PreferencesDto>;
}
