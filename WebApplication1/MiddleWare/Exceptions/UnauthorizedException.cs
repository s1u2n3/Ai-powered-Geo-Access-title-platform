namespace WebApplication1.MiddleWare.Exceptions
{
    public class UnauthorizedException
        : Exception
    {
        public UnauthorizedException(
            string message)
            : base(message)
        {
        }
    }
}
