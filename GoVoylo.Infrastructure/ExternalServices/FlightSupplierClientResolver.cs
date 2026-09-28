using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Interfaces;

namespace GoVoylo.Infrastructure.ExternalServices
{
    public class FlightSupplierClientResolver : IFlightSupplierClientResolver
    {
        private readonly IEnumerable<IFlightSupplierClient> _clients;

        public FlightSupplierClientResolver(IEnumerable<IFlightSupplierClient> clients)
        {
            _clients = clients;
        }

        public IFlightSupplierClient Resolve(string supplierCode)
        {
            var client = _clients.FirstOrDefault(c => c.SupplierCode == supplierCode);

            return client
                ?? throw new NotFoundException($"No flight supplier registered for code '{supplierCode}'.");
        }
    }
}
