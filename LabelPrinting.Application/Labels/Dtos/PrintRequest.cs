namespace LabelPrinting.Application.Labels.Dtos;

public class PrintRequest
{
    public Guid PrinterId { get; set; } // pdf or zebra
    public string LabelPath { get; set; } // path to label file
}
