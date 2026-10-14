using GoVoylo.Domain.Entities;

namespace GoVoylo.Domain.Interfaces
{
    public interface IAuthIdentityRepository
    {
        Task<AuthIdentity?> GetAsync(string provider, string providerSubject, CancellationToken cancellationToken);
        Task AddAsync(AuthIdentity identity, CancellationToken cancellationToken);
    }
}
