using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface IUserRoleRepository
    {
        Task<IReadOnlyList<string>> GetRoleNamesForUserAsync(Guid userId);
        Task<bool> HasRoleAsync(Guid userId, Guid roleId);
        Task AssignAsync(UserRole userRole);
    }
}
