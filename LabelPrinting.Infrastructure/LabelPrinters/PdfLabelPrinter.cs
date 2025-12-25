using LabelPrinting.Domain.Entities;
using LabelPrinting.Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZXing;
using ZXing.OneD;
using ZXing.Rendering;

namespace LabelPrinting.Infrastructure.Printers
{
    // PDF Label Configuration
    public class PdfLabelPrinter : ILabelPrinter
    {
        public void Print(Label label)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            // Convert inches to points (1 inch = 72 points)
            float width = 4 * 72;   // 4 inches
            float height = 6 * 72;  // 6 inches

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(width, height);   // Custom size
                    page.Margin(10);
                    page.Content().Column(col =>
                    {
                        col.Item().Text(label.ProductName).FontSize(20).Bold().AlignCenter();
                        col.Item().AlignCenter()
                        .AlignMiddle()
                        .Width(268)
                        .Height(50)
                        .Svg(size =>
                        {
                            var content = label.Barcode;
                            var writer = new Code128Writer(); // correct format for long strings
                            var eanCode = writer.encode(content, BarcodeFormat.CODE_128, (int)size.Width, (int)size.Height);
                            var renderer = new SvgRenderer { FontName = "Lato", FontSize = 16 }; // FontSize : is for the barcode text
                            return renderer.Render(eanCode, BarcodeFormat.CODE_128, content).Content;
                        });
                    });
                });
            }).GeneratePdf();

            File.WriteAllBytes($"{label.ProductName}.pdf", pdf);
        }
    }
}
