namespace GoVoylo.Application.Common.Exceptions
{
    // General-purpose 400 for a business rule violation that isn't a validation
    // failure on request shape (e.g. "hold_expired", "max_addresses_reached").
    public class BusinessRuleException : AppException
    {
        public override string Code { get; }
        public override int StatusCode => 400;

        public BusinessRuleException(string code, string message) : base(message)
        {
            Code = code;
        }
    }
}
