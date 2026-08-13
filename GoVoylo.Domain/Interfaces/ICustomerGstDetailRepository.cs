using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface ICustomerGstDetailRepository
    {
        Task<CustomerGstDetail?> GetByUserIdAsync(Guid userId);
        Task<bool> GstinExistsForOtherUserAsync(string gstin, Guid userId);
        Task AddAsync(CustomerGstDetail gstDetail);
        Task UpdateAsync(CustomerGstDetail gstDetail);
    }
}
