using LabelPrinting.Domain.Entities.Printer;
using Microsoft.EntityFrameworkCore;


namespace LabelPrinting.Infrastructure.Presistence
{
    internal class PrinterDBContext : DbContext
    {
        public PrinterDBContext(DbContextOptions<PrinterDBContext> options)
            : base(options)
        {
        }

        internal DbSet<Printer> Printer { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Printer>()
                .HasKey(p => p.PrinterId);

            base.OnModelCreating(modelBuilder);
        }
    }
}