namespace GoVoylo.Application.Common.Exceptions
{
    public abstract class AppException : Exception
    {
        public abstract string Code { get; }
        public abstract int StatusCode { get; }

        protected AppException(string message) : base(message)
        {
        }
    }
}
