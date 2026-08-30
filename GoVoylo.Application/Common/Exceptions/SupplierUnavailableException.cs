namespace GoVoylo.Application.Common.Exceptions
{
    // A downstream supplier (Tripjack) call failed — network error, timeout, or a
    // non-success response. 502, not 500: the fault is upstream, not in our own code.
    public class SupplierUnavailableException : AppException
    {
        public override string Code => "supplier_unavailable";
        public override int StatusCode => 502;

        public SupplierUnavailableException(string message) : base(message)
        {
        }
    }
}
