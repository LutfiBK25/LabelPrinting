using LabelPrinting.Domain.Entities.Label;
using LabelPrinting.Domain.Entities.Label.Elements;
using LabelPrinting.Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace LabelPrinting.Infrastructure.Printers;

// PDF Label Configuration
public class PdfLabelPrinter : ILabelPrinter
{
    private const float PointsPerInch = 72f; //(1 inch = 72 points)
    private const float DesignerScale = 100f; // Same scale used in designer (100 pixels per inch)
    public async Task Print(Label label)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        // Convert inches to points (1 inch = 72 points)
        float widthPoints = (float)label.LabelWidthInches * PointsPerInch;
        float heightPoints = (float)label.LabelHeightInches * PointsPerInch;

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(widthPoints, heightPoints);   // Custom size
                page.Margin(0); // No Margin
                page.Content().Layers(layers =>
                {
                    // Important: QuestPDF primary layer accepts a single direct child.
                    // The original approach risked assigning multiple children directly
                    // to the primary layer (or reusing the same child container across iterations),
                    // which triggers the "single-child container" exception and causes content
                    // to be overwritten.
                    //
                    // Fix applied:
                    // 1) Add exactly one child to the primary layer — here a Column container.
                    //    This guarantees the primary layer itself keeps a single child.
                    // 2) Let the Column own multiple items. Each label element is added as
                    //    its own child via `stack.Item().Element(...)`. That creates a new
                    //    single-child container for each element inside the Column, avoiding
                    //    reuse of the same container instance and preventing accidental overwrites.
                    //
                    // These steps ensure:
                    // - Only one direct child is attached to the primary layer.
                    // - Each element receives its own child container (safe for fluent API).
                    // - No container variable is reused outside its closure, avoiding scope issues.

                    layers.PrimaryLayer().Element(primary =>
                    {
                        primary.Column(stack =>
                        {
                            foreach (var serializableElement in label.Elements)
                            {
                                var element = serializableElement.ToDomain();

                                if (element is LabelTextElement textElement)
                                {
                                    float x = ConvertToPoints(element.X);
                                    float y = ConvertToPoints(element.Y);

                                    // Each item creates a fresh child container via Element(...)
                                    // so we never assign multiple children to a single-child container.
                                    // TranslateX/TranslateY are applied inside that child container,
                                    // ensuring each text element is positioned independently.
                                    stack.Item().Element(item =>
                                    {
                                        item.TranslateX(x).TranslateY(y)
                                            .Text(textElement.Text)
                                            .FontSize((float)textElement.FontSize)
                                            .FontColor(Colors.Black);
                                    });
                                }

                                // Add handling for other element types here
                            }
                        });
                    });
                });
            });
        }).GeneratePdf();

        // Save PDF to file
        string fileName = SanitizeFileName(label.Name);
        File.WriteAllBytes($"{fileName}.pdf", pdf);
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
