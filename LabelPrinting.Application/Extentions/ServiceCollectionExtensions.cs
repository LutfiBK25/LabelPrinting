using LabelPrinting.Application.LabelPrinters.Services;
using LabelPrinting.Application.Labels.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LabelPrinting.Application.Extentions;

public static class ServiceCollectionExtensions
{

    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILabelService, LabelService>();
        services.AddScoped<IPrinterService, PrinterService>();
    }

}
