using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface ICustomerAddressRepository
    {
        Task<CustomerAddress?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<CustomerAddress>> GetByUserIdAsync(Guid userId);
        Task<int> CountByUserIdAsync(Guid userId);
        Task AddAsync(CustomerAddress address);
        Task UpdateAsync(CustomerAddress address);
        Task DeleteAsync(CustomerAddress address);
        Task ClearDefaultForUserAsync(Guid userId);
    }
}
