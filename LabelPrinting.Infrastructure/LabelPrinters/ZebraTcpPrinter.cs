using LabelPrinting.Domain.Entities.Label;
using LabelPrinting.Domain.Entities.Label.Elements;
using LabelPrinting.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

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

        #region Rendering
        private void RenderElement(StringBuilder sb, LabelElement element)
        {
            if (element is LabelTextElement textElement)
            {
                RenderTextElement(sb, textElement);
            }
            if (element is LabelImageElement imageElement)
            {
                RenderImageElement(sb, imageElement);
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
            sb.AppendLine($"^CF0,{fontHeight}");
            sb.AppendLine($"^FO{x},{y}");           // Field Origin
            sb.AppendLine($"^FD{EscapeZplText(text.Text)}^FS"); // Field Data
            sb.AppendLine();
        }

        private void RenderImageElement(StringBuilder sb, LabelImageElement imageElement)
        {
            int x = ConvertToDots(imageElement.X);
            int y = ConvertToDots(imageElement.Y);
            sb.AppendLine($"^FO{x},{y}");
            if (string.IsNullOrEmpty(imageElement.Base64Image)) return;
            var bitmapImage = Base64ToBitmap(imageElement.Base64Image);
            using var mono = ConvertToMonochrome(bitmapImage);

            sb.Append(BuildZplImage(mono));
        }

        public static Bitmap Base64ToBitmap(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);

            using var ms = new MemoryStream(bytes);
            using var temp = new Bitmap(ms);
            return new Bitmap(temp);
        }

        Bitmap ConvertToMonochrome(Bitmap source)
        {
            Console.WriteLine($" Height:{source.Height}, Width: {source.Width}");
            var mono = new Bitmap(source.Width, source.Height, PixelFormat.Format1bppIndexed);

            var rect = new Rectangle(0, 0, source.Width, source.Height);
            var srcData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var dstData = mono.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

            try
            {
                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        int srcIndex = y * srcData.Stride + x * 4;

                        byte r = Marshal.ReadByte(srcData.Scan0, srcIndex + 2);
                        byte g = Marshal.ReadByte(srcData.Scan0, srcIndex + 1);
                        byte b = Marshal.ReadByte(srcData.Scan0, srcIndex);

                        int gray = (r + g + b) / 3;

                        if (gray < 128) // threshold
                        {
                            int dstIndex = y * dstData.Stride + (x >> 3);
                            byte mask = (byte)(0x80 >> (x & 7));
                            Marshal.WriteByte(
                                dstData.Scan0,
                                dstIndex,
                                (byte)(Marshal.ReadByte(dstData.Scan0, dstIndex) | mask)
                            );
                        }
                    }
                }
            }
            finally
            {
                source.UnlockBits(srcData);
                mono.UnlockBits(dstData);
            }

            return mono;
        }

        string BitmapToZplHex(Bitmap bitmap)
        {
            var bytesPerRow = (bitmap.Width + 7) / 8;
            var totalBytes = bytesPerRow * bitmap.Height;

            var hexBuilder = new StringBuilder(totalBytes * 2);

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

            try
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bytesPerRow; x++)
                    {
                        byte b = Marshal.ReadByte(
                            data.Scan0,
                            y * data.Stride + x
                        );

                        hexBuilder.Append(b.ToString("X2"));
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return hexBuilder.ToString();
        }

        string BuildZplImage(Bitmap bitmap)
        {
            int bytesPerRow = (bitmap.Width + 7) / 8;
            int totalBytes = bytesPerRow * bitmap.Height;

            string hex = BitmapToZplHex(bitmap);

            return $"""
            ^GFA,{totalBytes},{totalBytes},{bytesPerRow},{hex}
            """;
        }
        #endregion
    }
}
