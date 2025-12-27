namespace LabelPrinting.Domain.Entities.Label.Elements;

public class LabelTextElement : LabelElement
{
    public string Text { get; set; } = string.Empty;
    public double FontSize { get; set; }
}
