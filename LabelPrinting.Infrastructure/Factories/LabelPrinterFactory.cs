using LabelPrinting.Application.Printers.Interfaces;
using LabelPrinting.Domain.Entities;
using LabelPrinting.Domain.Interfaces;
using LabelPrinting.Infrastructure.Printers;

namespace LabelPrinting.Infrastructure.Factory
{
    // Used factory so fresh instance per request
    public class LabelPrinterFactory : ILabelPrinterFactory
    {
        public ILabelPrinter Create(Printer config)
        {
            return config.Type.ToLowerInvariant() switch
            {
                "pdf" => new PdfLabelPrinter(),
                "zebra" => new ZebraTcpPrinter(config.IP, config.Port),
                _ => throw new Exception("Unsupported printer type")
            };
        }
    }
}
