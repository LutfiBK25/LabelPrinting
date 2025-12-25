using LabelPrinting.Domain.Entities;

namespace LabelPrinting.Application.Labels.Services
{
    public interface ILabelService
    {
        Task PrintLabelAsync(Label label, Guid printerId);
    }
}
