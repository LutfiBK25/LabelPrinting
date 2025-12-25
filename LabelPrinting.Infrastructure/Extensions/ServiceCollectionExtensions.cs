using LabelPrinting.Application.Printers.Interfaces;
using LabelPrinting.Domain.Repositories;
using LabelPrinting.Infrastructure.Factory;
using LabelPrinting.Infrastructure.Presistence;
using LabelPrinting.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LabelPrinting.Infrastructure.Extensions
{
    // To extend from Infrastructure to API
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("PrintersDB");
            services.AddDbContext<PrinterDBContext>(options => options.UseSqlServer(connectionString));
            
            services.AddSingleton<ILabelPrinterFactory, LabelPrinterFactory>();
            services.AddScoped<IPrinterRepo, PrinterRepo>();
        }
    }
}
