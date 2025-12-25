using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelPrinting.Application.Labels.Dtos
{
    public class PrintRequest
    {
        public string ProductName { get; set; }
        public string Barcode { get; set; }
        public Guid PrinterId { get; set; } // pdf or zebra
    }
}
