using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LabelPrinting.Infrastructure.Presistence;

internal class PrinterDbContextFactory : IDesignTimeDbContextFactory<PrinterDBContext>
{
    public PrinterDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PrinterDBContext>();

        optionsBuilder.UseSqlServer(
            "Server=192.168.56.102,1433;Database=PrintersDB;User Id=sa;Password=2121998Bk21;TrustServerCertificate=True;"
        );

        return new PrinterDBContext(optionsBuilder.Options);
    }
}
