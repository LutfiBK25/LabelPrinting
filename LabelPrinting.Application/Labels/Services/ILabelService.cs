using LabelPrinting.Domain.Entities.Label;

namespace LabelPrinting.Application.Labels.Services
{
    public interface ILabelService
    {
        Task PrintLabelAsync(Label label, Guid printerId);
    }
}
