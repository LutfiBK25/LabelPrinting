using LabelPrinting.Domain.Entities.Label;
using LabelPrinting.Domain.Entities.Label.Elements;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static LabelPrinting.Domain.Entities.Label.Label;
using Label = LabelPrinting.Domain.Entities.Label.Label;

namespace LabelDesigner.Services;

public class LabelFileService
{
    private readonly Canvas _labelCanvas;
    private readonly Dictionary<UIElement, LabelElement> _elementMapping;

    public LabelFileService(
        Canvas labelCanvas,
        Dictionary<UIElement, LabelElement> elementMapping)
    {
        _labelCanvas = labelCanvas;
        _elementMapping = elementMapping;
    }
    // File Menu Methods

    // Save Design to JSON
    public void Save(Label label, double widthIn, double heightIn)
    {
        var saveDialog = new SaveFileDialog
        {
            Filter = "Label Design Files (*.lbl)|*.lbl",
            DefaultExt = ".lbl",
            FileName = label.Name
        };

        if (saveDialog.ShowDialog() != true)
            return;

        label.Elements.Clear();

        foreach (var kvp in _elementMapping)
        {
            var ui = kvp.Key;
            var domain = kvp.Value;

            domain.X = Canvas.GetLeft(ui);
            domain.Y = Canvas.GetTop(ui);

            if (ui is FrameworkElement fe)
            {
                domain.ElementWidth = fe.ActualWidth;
                domain.ElementHeight = fe.ActualHeight;
            }

            label.Elements.Add(SerializableLabelElement.FromDomain(domain));
        }

        label.LabelWidthInches = widthIn;
        label.LabelHeightInches = heightIn;

        File.WriteAllText(
            saveDialog.FileName,
            JsonSerializer.Serialize(label, new JsonSerializerOptions { WriteIndented = true })
        );
    }

    public Label? Load()
    {
        var openDialog = new OpenFileDialog
        {
            Filter = "Label Design Files (*.lbl)|*.lbl"
        };

        if (openDialog.ShowDialog() != true)
            return null;

        return JsonSerializer.Deserialize<Label>(
            File.ReadAllText(openDialog.FileName));
    }



}
