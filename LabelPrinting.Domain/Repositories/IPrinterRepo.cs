using LabelPrinting.Domain.Entities;

namespace LabelPrinting.Domain.Repositories
{
    public interface IPrinterRepo
    {
        Task<Printer?> GetPrinterByIdAsync(Guid printerId);
        Task<IEnumerable<Printer>> GetAllPrintersAsync();
        Task<Guid> Create(Printer printer);

    }
}
