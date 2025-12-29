using LabelPrinting.Application.LabelPrinters.Dtos;
using LabelPrinting.Application.LabelPrinters.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabelPrinting.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrinterController : ControllerBase
{
    private readonly IPrinterService _printerService;
    public PrinterController(IPrinterService printerService) 
    {
        _printerService = printerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPrinters()
    {
        var printers = await _printerService.GetAllPrinters();
        return Ok(printers);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPrinter([FromRoute] Guid id)
    {
        var printer = await _printerService.GetById(id);

        if (printer == null)
        {
            return NotFound();
        }
        return Ok(printer);
    }

    [HttpPost]
    public async Task<IActionResult> AddPrinter([FromBody] NewPrinterRequest request)
    {
        var id = await _printerService.AddPrinter(request);
        return CreatedAtAction(nameof(GetPrinter), new { id }, null);
    }

    [HttpPost("test/{id:guid}")]
    public async Task<IActionResult> TestPrinter([FromRoute] Guid id)
    {
        var printer = await _printerService.GetById(id);

        if (printer == null)
        {
            return NotFound();
        }

        var testConnetion = await _printerService.TestPrinter(printer.IP, printer.Port);
        if(!testConnetion)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "Printer is offline or unreachable"
            );

        }
        return Ok(testConnetion);
    }
}
