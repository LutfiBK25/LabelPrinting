using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LabelPrinting.Application.LabelPrinters.Dtos;
using LabelPrinting.Domain.Entities;
using LabelPrinting.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LabelPrinting.Application.LabelPrinters.Services
{
    internal class PrinterService : IPrinterService
    {
        private readonly IPrinterRepo _printerRepo;
        private readonly ILogger<PrinterService> _logger;
        
        public PrinterService(ILogger<PrinterService> logger, IPrinterRepo printerRepo)
        {
            _logger = logger;
            _printerRepo = printerRepo;
        }

        public async Task<Guid> AddPrinter(NewPrinterRequest request)
        {
            _logger.LogInformation("Adding a new printer");
            var newPrinter = NewPrinterRequest.FromDto(request);
            return await _printerRepo.Create(newPrinter);

        }

        public Task<IEnumerable<NewPrinterRequest>> DeletePrinter()
        {
            throw new NotImplementedException();
        }

        public async Task<PrinterDto> GetById(Guid id)
        {
            _logger.LogInformation("Getting printer with Id: {PrinterId}", id);

            var printer = await _printerRepo.GetPrinterByIdAsync(id);

            if (printer == null)
            {
                _logger.LogWarning($"Printer with Id {id} was not found");
                return null;
            }

            return PrinterDto.FromEntity(printer);

        }

        public async Task<IEnumerable<PrinterDto>> GetAllPrinters()
        {
            _logger.LogInformation("Getting All Printers");
            var printers = await _printerRepo.GetAllPrintersAsync();

            var response = printers.Select(PrinterDto.FromEntity);
            return response;
        }

        public async Task<bool> TestPrinter(string ip, int port)
        {
            int timeoutMs = 2000;
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(ip, port);

                if (await Task.WhenAny(connectTask, Task.Delay(timeoutMs)) != connectTask)
                {
                    return false;
                }
                return client.Connected;
            }
            catch (SocketException ex)
            {
                return false;
            }
        }

        public Task<IEnumerable<NewPrinterRequest>> UpdatePrinter()
        {
            throw new NotImplementedException();
        }
    }
}
