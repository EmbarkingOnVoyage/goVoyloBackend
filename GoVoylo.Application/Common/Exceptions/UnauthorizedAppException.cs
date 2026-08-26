namespace GoVoylo.Application.Common.Exceptions
{
    public class UnauthorizedAppException : AppException
    {
        public override string Code { get; }
        public override int StatusCode => 401;

        public UnauthorizedAppException(string code, string message) : base(message)
        {
            Code = code;
        }
    }
}
