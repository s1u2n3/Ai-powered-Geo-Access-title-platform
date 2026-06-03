using Microsoft.AspNetCore.Mvc;
using WebApplication1.Dto.Branch;
using WebApplication1.Dto.Company;
using WebApplication1.Services.Implementaions;
using WebApplication1.Services.Implementations;
using WebApplication1.Services.Interfaces;


[ApiController]
[Route("api/[controller]")]

    public class BranchController :ControllerBase
    {
    private readonly IBranchService _branchService;
    
        public BranchController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBranch data)
    {
        var result = await _branchService.CreateBranchAsync(data);
        return Ok(new
        {
            Message =
            "Branch created successfully"
        });
    }

}

