namespace LabelPrinting.Domain.Entities.Printer
{
    public class Printer
    {
        public Guid PrinterId { get; set; }
        public string Name { get; set; }
        public string IP {  get; set; }
        public int Port { get; set; }
        public string Type { get; set; }
        public bool Active { get; set; }
    }
}
