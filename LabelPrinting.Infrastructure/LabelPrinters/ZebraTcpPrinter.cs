using System;
using System.Net.Sockets;
using System.Text;
using LabelPrinting.Domain.Entities.Label;
using LabelPrinting.Domain.Entities.Label.Elements;
using LabelPrinting.Domain.Interfaces;

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

        public void Print(Label label)
        {
            try
            {
                using var client = new TcpClient(_ip, _port);
                using var stream = client.GetStream();

                string zpl = BuildZpl(label);
                var bytes = Encoding.ASCII.GetBytes(zpl);

                stream.Write(bytes, 0, bytes.Length);
            }
            catch (SocketException ex)
            {
                throw new Exception($"Cannot connect to Zebra printer at {_ip}:{_port}", ex);
            }
        }

        private string BuildZpl(Label label)
        {
            var sb = new StringBuilder();
            sb.AppendLine("^XA"); // start label

            // Set label width and length in dots
            sb.AppendLine($"^PW{ConvertToDots(label.WidthInches)}");
            sb.AppendLine($"^LL{ConvertToDots(label.HeightInches)}");

            // Loop through all label elements
            foreach (var serializableElement in label.Elements)
            {
                var element = serializableElement.ToDomain();

                if (element is LabelTextElement text)
                {
                    int x = ConvertToDots(text.X);
                    int y = ConvertToDots(text.Y);
                    int fontHeight = ConvertToDots(text.FontSize); // scale font size

                    // ^ADN,fontHeight,width -> width=0 lets printer scale proportionally
                    sb.AppendLine($"^FO{x},{y}^ADN,{fontHeight},0^FD{text.Text}^FS");
                }

                // TODO: add support for barcodes or images here
            }

            sb.AppendLine("^XZ"); // end label
            return sb.ToString();
        }

        private int ConvertToDots(double designerPixels)
        {
            // Convert from designer pixels to printer dots
            return (int)(designerPixels / DesignerScale * PrinterDpi);
        }
    }
}
