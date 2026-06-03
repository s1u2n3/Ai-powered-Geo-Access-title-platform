using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using WebApplication1.Dto;
using WebApplication1.GisEntities;
using WebApplication1.Services.Implementaions;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Controllers.PostGisController
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeoController : ControllerBase 
    {
        private readonly PostgresDbContext _context;
        private readonly IGeoService _service;
        public GeoController(PostgresDbContext context,IGeoService service)
        {
            _context = context;
            _service = service;
        }

        [Authorize]
        [HttpGet("parcels")]
        public async Task<IActionResult> GetParcels()
        {
            var parcels = await _context.Parcels.ToListAsync();
            

            return Ok(parcels);
        }
        [Authorize]
        [HttpPost("search")]
        public async Task<IActionResult> GetPropertyDetails( [FromBody] GetProprtyRequest request)
        {
            var parcelId = request.ParcelId;

            if (!string.IsNullOrWhiteSpace(parcelId))
            {
                var result =
                    await _service
                        .GetPropertyStaterAsync(parcelId);

                if (result != null)
                {
                    return Ok(result);
                }
            }

            return NotFound("invalid input");
        }
    }
}
