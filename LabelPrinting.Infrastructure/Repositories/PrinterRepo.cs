using LabelPrinting.Domain.Entities.Printer;
using LabelPrinting.Domain.Repositories;
using LabelPrinting.Infrastructure.Presistence;
using Microsoft.EntityFrameworkCore;

namespace LabelPrinting.Infrastructure.Repositories;

internal class PrinterRepo : IPrinterRepo
{
    private readonly PrinterDBContext _dbContext;

    public PrinterRepo(PrinterDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Create(Printer printer)
    {
        _dbContext.Printer.Add(printer);
        await _dbContext.SaveChangesAsync();
        return printer.PrinterId;
    }

    public async Task<IEnumerable<Printer>> GetAllPrintersAsync()
    {
        var printers = await _dbContext.Printer.ToListAsync();
        return printers;
    }

    public async Task<Printer?> GetPrinterByIdAsync(Guid printerId)
    {
        return await _dbContext.Printer
                         .AsNoTracking()
                         .FirstOrDefaultAsync(p => p.PrinterId == printerId);
    }
}
