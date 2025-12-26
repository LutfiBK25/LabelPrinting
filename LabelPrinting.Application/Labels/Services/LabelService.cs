using LabelPrinting.Application.Printers.Interfaces;
using LabelPrinting.Domain.Entities.Label;
using LabelPrinting.Domain.Interfaces;
using LabelPrinting.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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

    public async Task PrintLabelAsync(string labelPath, Guid printerId)
    {
        _logger.LogInformation("Print Label Service Triggered");

        _logger.LogInformation($"Loading label from path: {labelPath}");
        // Load label from file
        if (!File.Exists(labelPath))
        {
            throw new FileNotFoundException($"Label file not found at path: {labelPath}");
        }
        // To Do: Load label from file
        Label label;
        try
        {
            string json = await File.ReadAllTextAsync(labelPath);
            label = JsonSerializer.Deserialize<Label>(json);

            if (label == null)
            {
                throw new InvalidDataException("Failed to deserialize label file");
            }

            _logger.LogInformation($"Label loaded successfully: {label.Name} (ID: {label.Id})");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse label JSON file");
            throw new InvalidDataException("Invalid label file format", ex);
        }

        _logger.LogInformation($"Retrieving printer info for Printer ID: {printerId}");
        var printerInfo =  await _printerRepo.GetPrinterByIdAsync(printerId);
        if (printerInfo == null) throw new Exception("No Printer not found");

        _logger.LogInformation($"Printer info retrived, {printerInfo.Name}, {printerInfo.IP},{printerInfo.Port}");
        var printer = _printerFactory.Create(printerInfo);

        printer.Print(label);
    }
}
