using LabelPrinting.Domain.Entities.Label.Elements;

namespace LabelPrinting.Domain.Entities.Label;

public class Label
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<SerializableLabelElement> Elements { get; set; } = new List<SerializableLabelElement>();
    public double LabelWidthInches { get; set; }
    public double LabelHeightInches { get; set; }


    // Wrapper class for serialization
    public class SerializableLabelElement
    {
        public string Type { get; set; } = "Text";
        public Guid Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double ElementWidth { get; set; }
        public double ElementHeight { get; set; }

        // Text-specific properties
        public string? Text { get; set; }
        public double FontSize { get; set; }

        // Convert from domain entity, for serialization (Saving)
        public static SerializableLabelElement FromDomain(LabelElement element)
        {
            var serializable = new SerializableLabelElement
            {
                Id = element.Id,
                X = element.X,
                Y = element.Y,
                ElementWidth = element.ElementWidth,
                ElementHeight = element.ElementHeight
            };

            if (element is LabelTextElement textElement)
            {
                serializable.Type = "Text";
                serializable.Text = textElement.Text;
                serializable.FontSize = textElement.FontSize;
            }
            // Add more types here as needed

            return serializable;
        }

        // Convert to domain entity, for deserialization (Loading)
        public LabelElement ToDomain()
        {
            switch (Type)
            {
                case "Text":
                    return new LabelTextElement
                    {
                        Id = Id,
                        X = X,
                        Y = Y,
                        ElementWidth = ElementWidth,
                        ElementHeight = ElementHeight,
                        Text = Text ?? "Text",
                        FontSize = FontSize
                    };
                // Add more types here as needed
                default:
                    throw new NotSupportedException($"Element type '{Type}' is not supported");
            }
        }
    }
}
