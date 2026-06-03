using Microsoft.AspNetCore.Mvc;
using WebApplication1.Dto.Company;
using WebApplication1.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    // 🔹 GET ALL COMPANIES
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _companyService.GetAllAsync();
        return Ok(result);
    }

    // 🔹 CREATE COMPANY
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCompanyDto data)
    {
        var result = await _companyService.CreateAsync(data);
        return Ok(result);
    }

    [HttpGet("{companyName}")]
    public async Task<IActionResult> GetCompanyDetailsByName( string companyName)
    {
        var result = await _companyService.GetCompanyDetails(companyName);
        if(result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }
}