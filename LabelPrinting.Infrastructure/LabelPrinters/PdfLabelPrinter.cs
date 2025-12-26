using LabelPrinting.Domain.Entities.Label;
using LabelPrinting.Domain.Entities.Label.Elements;
using LabelPrinting.Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace LabelPrinting.Infrastructure.Printers
{
    // PDF Label Configuration
    public class PdfLabelPrinter : ILabelPrinter
    {
        private const float PointsPerInch = 72f; //(1 inch = 72 points)
        private const float DesignerScale = 100f; // Same scale used in designer (100 pixels per inch)
        public void Print(Label label)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            // Convert inches to points (1 inch = 72 points)
            float widthPoints = (float)label.WidthInches * PointsPerInch;
            float heightPoints = (float)label.HeightInches * PointsPerInch;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(widthPoints, heightPoints);   // Custom size
                    page.Margin(0); // No Margin
                    page.Content().Layers(layers =>
                    {
                        foreach (var serializableElement in label.Elements)
                        {
                            var element = serializableElement.ToDomain();
                            RenderElement(layers, element);
                        }
                    });
                });
            }).GeneratePdf();



            // Save PDF to file
            string fileName = SanitizeFileName(label.Name);
            File.WriteAllBytes($"{fileName}.pdf", pdf);
        }

        private void RenderElement(LayersDescriptor layers, LabelElement element)
        {
            float x = ConvertToPoints(element.X);
            float y = ConvertToPoints(element.Y);

            if (element is LabelTextElement textElement)
            {
                layers.PrimaryLayer().TranslateX(x).TranslateY(y)
                    .Text(textElement.Text)
                    .FontSize((float)textElement.FontSize)
                    .FontColor(Colors.Black);
            }
            // Add more element types here
        }

        // Convert designer pixels to PDF points
        private float ConvertToPoints(double designerPixels)
        {
            // Designer uses 100 pixels per inch
            // PDF uses 72 points per inch
            return (float)(designerPixels / DesignerScale * PointsPerInch);
        }


        // File name sanitizer to remove invalid characters
        private string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            return string.IsNullOrWhiteSpace(sanitized) ? "Label" : sanitized;
        }
    }
}
