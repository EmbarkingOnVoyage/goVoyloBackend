using System.Security.Claims;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Interfaces;

namespace GoVoylo.Api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                var claim = _httpContextAccessor.HttpContext?.User
                    .FindFirst(ClaimTypes.NameIdentifier);

                if (claim == null || !Guid.TryParse(claim.Value, out var userId))
                {
                    throw new UnauthorizedAppException(
                        "unauthenticated",
                        "No authenticated user found on the current request.");
                }

                return userId;
            }
        }
    }
}
