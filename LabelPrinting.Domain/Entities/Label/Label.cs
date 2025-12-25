using LabelPrinting.Domain.Entities.Label.Elements;

namespace LabelPrinting.Domain.Entities.Label;

public class Label
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<LabelElement> Elements { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

}
