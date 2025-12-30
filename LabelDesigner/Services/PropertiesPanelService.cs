using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LabelPrinting.Domain.Entities.Label.Elements;

namespace LabelDesigner.Services;

public class PropertiesPanelService
{
    private readonly Panel _panel;
    private readonly TextBox _x;
    private readonly TextBox _y;
    private readonly TextBox _width;
    private readonly TextBox _height;

    private bool _isUpdating;

    public PropertiesPanelService(
        Panel panel,
        TextBox x,
        TextBox y,
        TextBox width,
        TextBox height)
    {
        _panel = panel;
        _x = x;
        _y = y;
        _width = width;
        _height = height;
    }

    public void Update(UIElement? element, LabelElement? domain)
    {
        _isUpdating = true;

        if (element == null || domain == null)
        {
            Clear();
        }
        else if (element is FrameworkElement fe)
        {
            _panel.IsEnabled = true;
            _x.Text = Canvas.GetLeft(fe).ToString("F0");
            _y.Text = Canvas.GetTop(fe).ToString("F0");
            _width.Text = fe.ActualWidth.ToString("F0");
            _height.Text = fe.ActualHeight.ToString("F0");

            // Keep domain in sync
            domain.ElementWidth = fe.ActualWidth;
            domain.ElementHeight = fe.ActualHeight;
        }


        _isUpdating = false;
    }

    private void Clear()
    {
        _panel.IsEnabled = false;
        _x.Text = "0";
        _y.Text = "0";
        _width.Text = "0";
        _height.Text = "0";
    }

    public void ApplyChange(
        UIElement selected,
        LabelElement domain,
        TextBox source)
    {
        if (_isUpdating || string.IsNullOrWhiteSpace(source.Text))
            return;

        if (!double.TryParse(source.Text, out var value))
            return;

        bool isSize = source == _width || source == _height;
        if (isSize && value <= 0) return;
        if (!isSize && value < 0) return;

        if (source == _x)
        {
            Canvas.SetLeft(selected, value);
            domain.X = value;
        }
        else if (source == _y)
        {
            Canvas.SetTop(selected, value);
            domain.Y = value;
        }
        else if (source == _width && selected is FrameworkElement feW)
        {
            feW.Width = value;
            domain.ElementWidth = value;
        }
        else if (source == _height && selected is FrameworkElement feH)
        {
            feH.Height = value;
            domain.ElementHeight = value;
        }
    }

    public static void PreviewNumericInput(TextCompositionEventArgs e)
    {
        if (!char.IsDigit(e.Text, 0) && e.Text != "." && e.Text != "-")
            e.Handled = true;
    }
}
