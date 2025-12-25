using LabelPrinting.Domain.Entities.Printer;
using LabelPrinting.Domain.Interfaces;

namespace LabelPrinting.Application.Printers.Interfaces
{
    public interface ILabelPrinterFactory
    {
        ILabelPrinter Create(Printer printerConfig);
    }
}
