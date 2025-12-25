namespace LabelPrinting.Domain.Entities.Label.Elements;

public class LabelBarcodeElement : LabelElement
{
    public string BarcodeType { get; set; }
    public string Data { get; set; }
}
