using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost]
        public async Task<IActionResult>
            Create(
                CreateClientDto dto)
        {
            var result =
                await _service.CreateAsync(dto);

            return Ok(result);
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

    }
}