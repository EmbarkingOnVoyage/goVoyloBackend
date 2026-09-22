using GoVoylo.Application.Interfaces;

namespace GoVoylo.Infrastructure.ExternalServices.Payments;

public class PaymentProviderResolver : IPaymentProviderResolver
{
    private readonly IEnumerable<IPaymentProvider> _providers;

    public PaymentProviderResolver(
        IEnumerable<IPaymentProvider> providers)
    {
        _providers = providers;
    }

    public IPaymentProvider GetProvider(
        string providerName)
    {
        var provider = _providers.FirstOrDefault(
            x => string.Equals(
                x.ProviderName,
                providerName,
                StringComparison.OrdinalIgnoreCase));

        if (provider == null)
        {
            throw new InvalidOperationException(
                $"Payment provider '{providerName}' is not supported.");
        }

        return provider;
    }
}