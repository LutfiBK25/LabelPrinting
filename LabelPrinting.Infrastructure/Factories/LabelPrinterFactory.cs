using LabelPrinting.Application.Printers.Interfaces;
using LabelPrinting.Domain.Entities.Printer;
using LabelPrinting.Domain.Interfaces;
using LabelPrinting.Infrastructure.Printers;

namespace LabelPrinting.Infrastructure.Factory
{
    // Used factory so fresh instance per request
    public class LabelPrinterFactory : ILabelPrinterFactory
    {
        public ILabelPrinter Create(Printer printer)
        {
            return printer.Type.ToLowerInvariant() switch
            {
                "pdf" => new PdfLabelPrinter(),
                "zebra" => new ZebraTcpPrinter(printer.IP, printer.Port),
                _ => throw new Exception("Unsupported printer type")
            };
        }
    }
}
