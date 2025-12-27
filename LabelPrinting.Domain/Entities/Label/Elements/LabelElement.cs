namespace LabelPrinting.Domain.Entities.Label.Elements;

public abstract class LabelElement
{
    public Guid Id { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double ElementWidth { get; set; }
    public double ElementHeight { get; set; }
}
