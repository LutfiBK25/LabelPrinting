using LabelPrinting.Domain.Entities.Printer;

namespace LabelPrinting.Application.LabelPrinters.Dtos;

public class PrinterDto
{
    public Guid PrinterId { get; set; }
    public string Name { get; set; }
    public string IP { get; set; }
    public int Port { get; set; }
    public string Type { get; set; }
    public bool Active { get; set; }


    public static PrinterDto? FromEntity(Printer? printer)
    {
        if(printer == null) throw new ArgumentNullException(nameof(printer));
        return new PrinterDto
        {
            PrinterId = printer.PrinterId,
            Name = printer.Name,
            IP = printer.IP,
            Port = printer.Port,
            Type = printer.Type,
            Active = printer.Active,
        };
    }


}
