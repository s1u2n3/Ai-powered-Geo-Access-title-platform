using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MimeKit;
using System.Security.Claims;
using WebApplication1.Dto;
using WebApplication1.Dto.Client;
using WebApplication1.Services.Implementations;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController
        : ControllerBase
    {
        private readonly IClientService
            _service;

        public ClientController(
            IClientService service)
        {
            _service = service;
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult>
            Create(
                CreateClientDto dto)
        {
            var UserId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result =
                await _service.CreateAsync(dto, UserId);
            if(result == null)
            {
                return  null;
            }
            return Ok("client created successfully");
        }
        [Authorize]
        [HttpGet("client-details")]
        public async Task<IActionResult>
            GetClientDetails()
        {
            var response =
                await _service
                .GetClientDetailsAsync(User);

            return Ok(response);
        }
        [HttpGet("redis-test")]
        public async Task<IActionResult> RedisTest(
    [FromServices] IDistributedCache cache)
        {
            await cache.SetStringAsync(
                "test",
                "Hello Redis");

            var value =
                await cache.GetStringAsync("test");

            return Ok(value);
        }

    }
}