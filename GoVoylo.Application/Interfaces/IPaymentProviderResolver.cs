using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Interfaces
{
    public interface IPaymentProviderResolver
    {
        IPaymentProvider GetProvider(string providerName);
    }
}
