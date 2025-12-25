using LabelPrinting.Application.Printers.Interfaces;
using LabelPrinting.Domain.Entities;
using LabelPrinting.Domain.Interfaces;
using LabelPrinting.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LabelPrinting.Application.Labels.Services;

public class LabelService : ILabelService
{
    private readonly IPrinterRepo _printerRepo;
    private readonly ILabelPrinterFactory _printerFactory;
    private readonly ILogger<LabelService> _logger;

    public LabelService(IPrinterRepo printerConfigRepo, ILabelPrinterFactory printerFactory, ILogger<LabelService> logger)
    {
        _printerRepo = printerConfigRepo;
        _printerFactory = printerFactory;
        _logger = logger;
    }

    public async Task PrintLabelAsync(Label label, Guid printerId)
    {
        _logger.LogInformation("Label Service Triggered");
        var printerInfo =  await _printerRepo.GetPrinterByIdAsync(printerId);
        if (printerInfo == null) throw new Exception("No Printer not found");

        _logger.LogInformation($"Printer info retrived, {printerInfo.Name}, {printerInfo.IP},{printerInfo.Port}");
        var printer = _printerFactory.Create(printerInfo);

        printer.Print(label);
    }
}
