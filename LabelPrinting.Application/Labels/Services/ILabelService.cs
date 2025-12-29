namespace LabelPrinting.Application.Labels.Services;

public interface ILabelService
{
    Task PrintLabelAsync(string labelPath, Guid printerId);
}
