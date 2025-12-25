using LabelPrinting.Domain.Entities;

namespace LabelPrinting.Domain.Interfaces
{
    // Interface for the type of the printers it can be
    public interface ILabelPrinter
    {
        void Print(Label label);
    }
}
