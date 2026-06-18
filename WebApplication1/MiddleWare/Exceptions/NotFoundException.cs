namespace WebApplication1.MiddleWare.Exceptions
{
    public class NotFoundException:Exception
    {
        public NotFoundException(
            string message)
            : base(message)
        {
        }
    }
}
