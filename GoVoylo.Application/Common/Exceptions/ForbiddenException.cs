namespace GoVoylo.Application.Common.Exceptions
{
    public class ForbiddenException : AppException
    {
        public override string Code { get; }
        public override int StatusCode => 403;

        public ForbiddenException(string code, string message) : base(message)
        {
            Code = code;
        }
    }
}
