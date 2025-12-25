using LabelPrinting.Domain.Entities.Printer;

namespace LabelPrinting.Domain.Repositories
{
    public interface IPrinterRepo
    {
        Task<Printer?> GetPrinterByIdAsync(Guid printerId);
        Task<IEnumerable<Printer>> GetAllPrintersAsync();
        Task<Guid> Create(Printer printer);

    }
}
