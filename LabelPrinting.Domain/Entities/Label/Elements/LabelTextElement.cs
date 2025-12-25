using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelPrinting.Domain.Entities.Label.Elements;

public class LabelTextElement : LabelElement
{
    public string Text { get; set; }
    public string FontFamily { get; set; }
    public double FontSize { get; set; }
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public bool IsUnderline { get; set; }
}
