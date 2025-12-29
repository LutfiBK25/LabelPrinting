using LabelPrinting.Application.Labels.Dtos;
using LabelPrinting.Application.Labels.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabelPrinting.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LabelController : ControllerBase
{
    private readonly ILabelService _labelService;

    public LabelController(ILabelService labelService)
    {
        _labelService = labelService;
    }

    [HttpPost("print")]
    public async Task<IActionResult> Print([FromBody] PrintRequest request)
    {
        try
        {
            await _labelService.PrintLabelAsync(request.LabelPath, request.PrinterId);
            return Ok("Label sent to printer");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}