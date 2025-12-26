using LabelPrinting.Domain.Entities.Label.Elements;

namespace LabelPrinting.Domain.Entities.Label;

public class Label
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<SerializableLabelElement> Elements { get; set; } = new List<SerializableLabelElement>();
    public double WidthInches { get; set; }
    public double HeightInches { get; set; }


    // Wrapper class for serialization
    public class SerializableLabelElement
    {
        public string Type { get; set; } = "Text";
        public Guid Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        // Text-specific properties
        public string? Text { get; set; }
        public double FontSize { get; set; }

        // Future: Image-specific properties / Find Another Way like how does Bartender do it, can we serailize images?
        public string? ImagePath { get; set; }

        // Convert from domain entity, for serialization (Saving)
        public static SerializableLabelElement FromDomain(LabelElement element)
        {
            var serializable = new SerializableLabelElement
            {
                Id = element.Id,
                X = element.X,
                Y = element.Y,
                Width = element.Width,
                Height = element.Height
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
                        Width = Width,
                        Height = Height,
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
