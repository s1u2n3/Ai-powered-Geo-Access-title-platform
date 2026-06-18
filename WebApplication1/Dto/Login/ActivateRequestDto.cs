using Org.BouncyCastle.Crypto;

namespace WebApplication1.Dto.Login
{
    public class ActivateRequestDto
    {
        public string token { get; set; }
        public  string Password{ get; set; }

        public string ConfirmPassword { get; set; }
    }
}
