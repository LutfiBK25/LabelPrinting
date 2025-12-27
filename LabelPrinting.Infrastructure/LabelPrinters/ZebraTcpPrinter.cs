using System;
using System.Net.Sockets;
using System.Text;
using LabelPrinting.Domain.Entities.Label;
using LabelPrinting.Domain.Entities.Label.Elements;
using LabelPrinting.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LabelPrinting.Infrastructure.Printers
{
    public class ZebraTcpPrinter : ILabelPrinter
    {
        private readonly string _ip;
        private readonly int _port;
        private const int PrinterDpi = 203;       // Zebra printer dots per inch
        private const double DesignerScale = 100; // Pixels per inch in your designer

        public ZebraTcpPrinter(string ip, int port)
        {
            _ip = ip;
            _port = port;

        }

        public async Task Print(Label label)
        {
            try
            {
                // Connect to the Zebra printer via TCP
                using var client = new TcpClient();
                await client.ConnectAsync(_ip, _port);

                // Get the network stream for sending data
                using var stream = client.GetStream();

                // Build ZPL command string from the label
                string zpl = BuildZpl(label);
                var bytes = Encoding.ASCII.GetBytes(zpl);

                // Send ZPL commands to the printer
                await stream.WriteAsync(bytes, 0, bytes.Length);
                // Ensure all data is sent
                await stream.FlushAsync();
            }
            catch (SocketException ex)
            {
                throw new Exception($"Cannot connect to Zebra printer at {_ip}:{_port}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error printing to Zebra printer: {ex.Message}", ex);
            }
        }

        private string BuildZpl(Label label)
        {
            var sb = new StringBuilder();
            sb.AppendLine("^XA"); // start label

            // Set label dimensions in dots
            int widthDots = ConvertToDots(label.LabelWidthInches * DesignerScale);
            int heightDots = ConvertToDots(label.LabelHeightInches * DesignerScale);

            sb.AppendLine($"^PW{widthDots}");    // Print width
            sb.AppendLine($"^LL{heightDots}");   // Label length

            // Set print speed and darkness (optional)
            sb.AppendLine("^PR4");  // Print speed (2-14, 4 is medium)
            sb.AppendLine("^MD15"); // Media darkness (0-30, 15 is medium)


            // Loop through all label elements
            foreach (var serializableElement in label.Elements)
            {
                var element = serializableElement.ToDomain();
                RenderElement(sb, element);
            }

            sb.AppendLine("^XZ"); // end label

            return sb.ToString();
        }

        private void RenderElement(StringBuilder sb, LabelElement element)
        {
            if (element is LabelTextElement textElement)
            {
                RenderTextElement(sb, textElement);
            }
            // Add more element types as needed
        }

        private void RenderTextElement(StringBuilder sb, LabelTextElement text)
        {
            int x = ConvertToDots(text.X);
            int y = ConvertToDots(text.Y);
            int fontHeight = ConvertToDots(text.FontSize);

            // Use ^A command for font selection
            // ^ADN = Font D (sans serif), Normal orientation
            // You can change to ^A0N (font 0), ^AAN (font A), etc.
            sb.AppendLine($"^FO{x},{y}");           // Field Origin
            sb.AppendLine($"^ADN,{fontHeight},0");   // Font command
            sb.AppendLine($"^FD{EscapeZplText(text.Text)}^FS"); // Field Data
        }

        private int ConvertToDots(double designerPixels)
        {
            // Convert from designer pixels to printer dots
            return (int)(designerPixels / DesignerScale * PrinterDpi);
        }

        private string EscapeZplText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Escape special ZPL characters
            return text
                .Replace("^", "^FH^5E") // Caret
                .Replace("~", "^FH^7E") // Tilde
                .Replace("\\", "^FH^5C"); // Backslash
        }
    }
}
