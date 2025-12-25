using LabelPrinting.Domain.Entities.Printer;

namespace LabelPrinting.Application.LabelPrinters.Dtos
{
    public class NewPrinterRequest
    {
        public string Name { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public string Type { get; set; }
        public bool Active { get; set; }
    
    
        public static Printer FromDto(NewPrinterRequest newPrinterRequest)
        {
            if (newPrinterRequest == null) throw new ArgumentNullException(nameof(newPrinterRequest));
            return new Printer
            {
                Name = newPrinterRequest.Name,
                IP = newPrinterRequest.IP,
                Port = newPrinterRequest.Port,
                Type = newPrinterRequest.Type,
                Active = newPrinterRequest.Active,
            };
        }
    }
}
