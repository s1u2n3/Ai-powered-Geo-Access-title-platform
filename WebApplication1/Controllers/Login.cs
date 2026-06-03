using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication1.Dto.Login ;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class AuthController : ControllerBase
        

    {

        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthController(IConfiguration configuration ,AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        
        }

        [HttpPost("admin-login")]
        public IActionResult AdminLogin(Login dto)
        {
            if (dto.Email == "admin@test.com"
                &&
                dto.Password == "123")
            {
                var claims = new[]
                {
            new Claim(
                ClaimTypes.Email,
                dto.Email),

            new Claim(
                ClaimTypes.Role,
                "Admin")
        };
                var key =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            _configuration["Jwt:Key"]));


                var credentials =
                    new SigningCredentials(
                        key,
                        SecurityAlgorithms.HmacSha256);
                var token =
                    new JwtSecurityToken(
                        issuer:
                            _configuration["Jwt:Issuer"],

                        audience:
                            _configuration["Jwt:Audience"],

                        claims: claims,

                        expires:
                            DateTime.UtcNow.AddHours(1),

                        signingCredentials:
                            credentials);
           

                var jwt =
                    new JwtSecurityTokenHandler()
                    .WriteToken(token);

                return Ok(jwt);
            }

            return Unauthorized(
                "Invalid Credentials");
        }
        [HttpPost("client-login")]
        public async Task<IActionResult> ClientLogin (Login dto)
        {
            // STEP 1
            // find client in DB
            var hash =
BCrypt.Net.BCrypt.HashPassword(
    "Client123");

            Console.WriteLine(hash);
            var client =
                await _context.Clients
                .FirstOrDefaultAsync(x =>
                    x.Email == dto.Email);

            // STEP 2
            // check email exists

            if (client == null)
            {
                return Unauthorized(
                    "Invalid Email");
            }

            // STEP 3
            // verify password hash

            var isPasswordValid =
                BCrypt.Net.BCrypt.Verify(
                    dto.Password,
                    client.PasswordHash);

            if (!isPasswordValid)
            {
                return Unauthorized(
                    "Invalid Password");
            }

            // STEP 4
            // create claims

            var claims = new[]
            {
            new Claim(
                ClaimTypes.Email,
                client.Email),

            new Claim(
                "ClientId",
                client.Id.ToString()),

            new Claim(
                "UserType",
                "Client")
        };

            // STEP 5
            // create signing key

            var key =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        _configuration["Jwt:Key"]));

            // STEP 6
            // signing credentials

            var credentials =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256);

            // STEP 7
            // create JWT token

            var token =
                new JwtSecurityToken(
                    issuer:
                        _configuration["Jwt:Issuer"],

                    audience:
                        _configuration["Jwt:Audience"],

                    claims: claims,

                    expires:
                        DateTime.UtcNow.AddHours(1),

                    signingCredentials:
                        credentials);

            // STEP 8
            // convert token to string

            var jwt =
                new JwtSecurityTokenHandler()
                .WriteToken(token);

            // STEP 9
            // return token

            return Ok(new
            {
                Token = jwt,

                UserType = "Client",

                ClientId = client.Id,

                Name = client.ClientName,

                EmailId = client.Email,

                Expiration =
         DateTime.UtcNow.AddHours(1)
            });
        }

    }
}