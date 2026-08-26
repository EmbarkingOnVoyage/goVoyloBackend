using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class CustomerGstDetailRepository : ICustomerGstDetailRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerGstDetailRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerGstDetail?> GetByUserIdAsync(Guid userId)
        {
            return await _context.CustomerGstDetails.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<bool> GstinExistsForOtherUserAsync(string gstin, Guid userId)
        {
            return await _context.CustomerGstDetails
                .AnyAsync(x => x.Gstin == gstin && x.UserId != userId);
        }

        public async Task AddAsync(CustomerGstDetail gstDetail)
        {
            await _context.CustomerGstDetails.AddAsync(gstDetail);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CustomerGstDetail gstDetail)
        {
            _context.CustomerGstDetails.Update(gstDetail);
            await _context.SaveChangesAsync();
        }
    }
}
