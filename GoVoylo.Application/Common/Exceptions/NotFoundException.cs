namespace GoVoylo.Application.Common.Exceptions
{
    public class NotFoundException : AppException
    {
        public override string Code => "not_found";
        public override int StatusCode => 404;

        public NotFoundException(string message) : base(message)
        {
        }
    }
}
