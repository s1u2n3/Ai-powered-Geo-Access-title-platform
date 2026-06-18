using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using WebApplication1.Dto.Login ;
using WebApplication1.MiddleWare.Exceptions;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class AuthController : ControllerBase


    {

        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(Login dto)
        {
            // 1. Check Users table first
          


            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user != null)
            {
               
                                    var isPasswordValid =
                    BCrypt.Net.BCrypt.Verify(
                        dto.Password,
                        user.PasswordHash);

                if (!isPasswordValid)
                {
                     throw new UnauthorizedException(
                     "Invalid credentials");
                }

                var claims = new[]
                {
            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(
                ClaimTypes.Role,
                user.Role),

            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),
           
            new Claim(
                "UserType",
                "User")
        };

                var jwt = GenerateJwt(claims);

                return Ok(new
                {
                    Token = jwt,
                    UserType = "User",
                    Role = user.Role,
                    UserId = user.Id,
                    Name = user.Name,
                    EmailId = user.Email,
                    Expiration = DateTime.UtcNow.AddHours(1)
                });
            }

            // 2. Check Clients table

            var client = await _context.Clients
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (client != null)
            {
                var isPasswordValid =
                    BCrypt.Net.BCrypt.Verify(
                        dto.Password,
                        client.PasswordHash);

                if (!isPasswordValid)
                {
                    return Unauthorized("Invalid Password");
                }

                var claims = new[]
                {
            new Claim(
                ClaimTypes.Email,
                client.Email),

            new Claim(
                ClaimTypes.Role,
                "Client"),

            new Claim(
                ClaimTypes.NameIdentifier,
                client.Id.ToString()),

            new Claim(
                "UserType",
                "Client")
        };

                var jwt = GenerateJwt(claims);

                return Ok(new
                {
                    Token = jwt,
                    UserType = "Client",
                    Role = "Client",
                    ClientId = client.Id,
                    Name = client.ClientName,
                    EmailId = client.Email,
                    Expiration = DateTime.UtcNow.AddHours(1)
                });
            }

            return Unauthorized("Invalid Credentials");
        }
        private string GenerateJwt(IEnumerable<Claim> claims)
        {
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
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

       

        [HttpPost("activate-client")]
        public async Task<IActionResult> ActivateClient(
    ActivateRequestDto req)
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(x =>
                    x.EmailVerificationToken == req.token);

            if (client == null)
            {
                return BadRequest("Client not found");
            }

            if (req.Password != req.ConfirmPassword)
            {
                return BadRequest("Password mismatch");
            }

            if (client.VerificationTokenExpiry < DateTime.UtcNow)
            {
                return BadRequest("Token expired");
            }

            client.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(req.Password);

            client.IsEmailVerified = true;

            client.EmailVerificationToken = null;

            client.VerificationTokenExpiry = null;

            await _context.SaveChangesAsync();

            var claims = new[]
            {
        new Claim(
            ClaimTypes.Email,
            client.Email),

        new Claim(
            ClaimTypes.Role,
            "Client"),

        new Claim(
            "ClientId",
            client.Id.ToString()),

        new Claim(
            "UserType",
            "Client")
    };

            var jwt = GenerateJwt(claims);

            return Ok(new
            {
                Token = jwt,
                UserType = "Client",
                Role = "Client",
                ClientId = client.Id,
                Name = client.ClientName,
                EmailId = client.Email,
                Expiration = DateTime.UtcNow.AddHours(1)
            });
        }



    }
}