using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class AuthIdentityRepository : IAuthIdentityRepository
    {
        private readonly ApplicationDbContext _context;

        public AuthIdentityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<AuthIdentity?> GetAsync(string provider, string providerSubject, CancellationToken cancellationToken) =>
            _context.AuthIdentities.FirstOrDefaultAsync(
                x => x.Provider == provider && x.ProviderSubject == providerSubject, cancellationToken);

        public async Task AddAsync(AuthIdentity identity, CancellationToken cancellationToken)
        {
            _context.AuthIdentities.Add(identity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
