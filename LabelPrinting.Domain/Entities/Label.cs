namespace LabelPrinting.Domain.Entities;

public class Label
{
    public string ProductName { get; set; }
    public string Barcode { get; set; }

    public Label(string productName, string barcode)
    {
        ProductName = productName;
        Barcode = barcode;
    }
}
