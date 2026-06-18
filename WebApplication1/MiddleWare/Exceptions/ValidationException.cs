namespace WebApplication1.MiddleWare.Exceptions
{
    public class ValidationException
        : Exception
    {
        public ValidationException(
            string message)
            : base(message)
        {
        }
    }
}
