using LabelPrinting.Application.LabelPrinters.Dtos;

namespace LabelPrinting.Application.LabelPrinters.Services;

public interface IPrinterService
{
    Task<IEnumerable<PrinterDto>> GetAllPrinters();
    Task<PrinterDto> GetById(Guid guid);
    Task<Guid> AddPrinter(NewPrinterRequest request);
    Task<IEnumerable<NewPrinterRequest>> UpdatePrinter();
    Task<IEnumerable<NewPrinterRequest>> DeletePrinter();
    Task<bool> TestPrinter(string ip, int port);

}
