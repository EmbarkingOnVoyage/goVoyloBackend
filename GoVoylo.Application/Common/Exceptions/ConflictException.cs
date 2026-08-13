namespace GoVoylo.Application.Common.Exceptions
{
    public class ConflictException : AppException
    {
        public override string Code { get; }
        public override int StatusCode => 409;

        public ConflictException(string code, string message) : base(message)
        {
            Code = code;
        }
    }
}
