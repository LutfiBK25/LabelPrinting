using System.Net.Sockets;
using System.Text;
using LabelPrinting.Domain.Entities;
using LabelPrinting.Domain.Interfaces;

namespace LabelPrinting.Infrastructure.Printers
{
    // Zebra Label Configuration
    public class ZebraTcpPrinter : ILabelPrinter
    {
        private readonly string _ip;
        private readonly int _port;

        public ZebraTcpPrinter(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public void Print(Label label)
        {
            try
            {
                using var client = new TcpClient(_ip, _port); // each print open a new tcp connection
                using var stream = client.GetStream(); // tcp data pipe , write raw bytes to it

                string zpl =
                    $@"^XA
                    ^PW812
                    ^LL1218
                    ^FO102,102^ADN,50,30^FD{label.ProductName}^FS
                    ^FO102,200^BY3
                    ^BCN,200,Y,N,N
                    ^FD{label.Barcode}^FS
                    ^XZ";

                var bytes = Encoding.ASCII.GetBytes(zpl);
                stream.WriteAsync(bytes, 0, bytes.Length);
            }
            catch (SocketException ex)
            {
                /// ToDo program shouldn't be crashing
                // when printer is offline (if there is no printer it will crash)
                throw new Exception($"Cannot connect to Zebra printer at {_ip}:{_port}", ex);
            }
        }
    }
}
