using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelPrinting.Domain.Entities.Label.Elements;

public class LabelTextElement : LabelElement
{
    public string Text { get; set; }
    public double FontSize { get; set; }
}
