using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class CustomerAddressRepository : ICustomerAddressRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerAddressRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerAddress?> GetByIdAsync(Guid id)
        {
            return await _context.CustomerAddresses.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IReadOnlyList<CustomerAddress>> GetByUserIdAsync(Guid userId)
        {
            return await _context.CustomerAddresses
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public Task<int> CountByUserIdAsync(Guid userId)
        {
            return _context.CustomerAddresses.CountAsync(x => x.UserId == userId);
        }

        public async Task AddAsync(CustomerAddress address)
        {
            await _context.CustomerAddresses.AddAsync(address);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CustomerAddress address)
        {
            _context.CustomerAddresses.Update(address);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(CustomerAddress address)
        {
            _context.CustomerAddresses.Remove(address);
            await _context.SaveChangesAsync();
        }

        public async Task ClearDefaultForUserAsync(Guid userId)
        {
            var addresses = await _context.CustomerAddresses
                .Where(x => x.UserId == userId && x.IsDefault)
                .ToListAsync();

            foreach (var address in addresses)
            {
                address.SetAsDefault(false);
            }

            if (addresses.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}
