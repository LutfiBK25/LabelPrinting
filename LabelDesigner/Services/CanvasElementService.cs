using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace LabelDesigner.Services;

/// <summary>
/// Service for managing canvas element interactions (drag, resize, edit, highlight).
/// Centralizes all UI element manipulation logic separated from the MainWindow code-behind.
/// </summary>
public class CanvasElementService
{
    private readonly Canvas _labelCanvas;
    private readonly Action<UIElement> _onSelectionChanged;
    private readonly Action<UIElement>? _onElementMoved;
    private readonly Action<UIElement>? _onElementSizeChanged;
    private Point _dragStartPoint = default;

    public CanvasElementService(Canvas labelCanvas, Action<UIElement> onSelectionChanaged, Action<UIElement>? onElementMoved, Action<UIElement>? onElementSizeChanged)
    {
        _labelCanvas = labelCanvas;
        _onSelectionChanged = onSelectionChanaged;
        _onElementMoved = onElementMoved;
        _onElementSizeChanged = onElementSizeChanged;
    }

    /// <summary>
    /// Clamp all canvas elements to stay within canvas bounds (for resizing Label).
    /// </summary>
    public void ClampElementsToCanvas()
    {
        foreach (UIElement element in _labelCanvas.Children)
        {
            if (element is FrameworkElement fe)
            {
                double left = Canvas.GetLeft(fe);
                double top = Canvas.GetTop(fe);

                // Handle NaN values
                if (double.IsNaN(left)) left = 0;
                if (double.IsNaN(top)) top = 0;

                // Clamp positions
                left = Math.Max(0, Math.Min(left, _labelCanvas.Width - fe.ActualWidth));
                top = Math.Max(0, Math.Min(top, _labelCanvas.Height - fe.ActualHeight));

                Canvas.SetLeft(fe, left);
                Canvas.SetTop(fe, top);
            }
        }
    }

    /// <summary>
    /// Highlight a selected element and clear highlights from others.
    /// </summary>
    public void HighlightSelectedElement(UIElement element)
    {
        if (element is null) return;
        // Clear all highlights
        var adornerLayer = AdornerLayer.GetAdornerLayer(element);
        if (adornerLayer != null)
        {
            adornerLayer.Add(new TransformAdorner(element, _onElementSizeChanged));
        }
    }

    /// <summary>
    /// Make a UI element draggable within the canvas with boundary clamping(checks)
    /// </summary>
    // UIElement is base class for most of the visual stuff
    public void MakeDraggable(UIElement element)
    {
        // Subscribe to the press of left mouse button
        element.MouseLeftButtonDown += (s, e) =>
        {
            // Don't start dragging if the textbox is in edit mode
            if (element is TextBox tb && !tb.IsReadOnly) return;
            // Mouse position related to label canvas (assigned when calling the service)
            _dragStartPoint = e.GetPosition(_labelCanvas);
            // element capture all mouse event (in this case as long as left mouse button is pressed)
            element.CaptureMouse();
            // Make the element as selected
            _onSelectionChanged(element);
            // prevent the parent control of reacting (Dont let anyone response to the event beside me)
            e.Handled = true;
        };

        // Subscribe to mouse click released
        element.MouseLeftButtonUp += (s, e) =>
        {
            // Release the capture
            element.ReleaseMouseCapture();
            // prevent the parent control of reacting (Dont let anyone response to the event beside me)
            e.Handled = true;
        };

        // As long as the mouse is moving
        element.MouseMove += (s, e) =>
        {
            // Check if THIS element has mouse capture (is being dragged)
            if (!element.IsMouseCaptured)
                return;

            // Don't drag if in edit mode (extra protection)
            if (element is TextBox tb && !tb.IsReadOnly)
                return;

            // current position of the mouse to the canvas
            var position = e.GetPosition(_labelCanvas);
            // calculate the mouse movement delta
            double offsetX = position.X - _dragStartPoint.X;
            double offsetY = position.Y - _dragStartPoint.Y;

            // Get current position of the element on the canvas 
            // it will be the label canvas in our case
            double currentLeft = Canvas.GetLeft(element);
            double currentTop = Canvas.GetTop(element);

            // Default position is 0 if not set, return NaN
            if (double.IsNaN(currentLeft)) currentLeft = 0;
            if (double.IsNaN(currentTop)) currentTop = 0;

            // Get new the location based on Element location and the mousemovement on the element
            double newLeft = currentLeft + offsetX;
            double newTop = currentTop + offsetY;

            // Clamp inside canvas (no leaving the Canvas)
            if (element is FrameworkElement fe)
            {
                newLeft = Math.Max(0, Math.Min(newLeft, _labelCanvas.Width - fe.ActualWidth));
                newTop = Math.Max(0, Math.Min(newTop, _labelCanvas.Height - fe.ActualHeight));
            }
            // Applies the new position to the element
            Canvas.SetLeft(element, newLeft);
            Canvas.SetTop(element, newTop);

            _onElementMoved?.Invoke(element);

            // keep the drag start point with the mouse movement
            _dragStartPoint = position;
            e.Handled = true;
        };
    }



    #region Text Box
    /// <summary>
    /// Make a TextBox editable on double-click and handle focus/edit state.
    /// </summary>
    public void MakeTextBoxEditable(TextBox textBox, Action<UIElement?> onEditEnd)
    {
        // Set initial cursor to arrow (for dragging)
        textBox.Cursor = Cursors.Arrow;

        textBox.MouseDoubleClick += (s, e) =>
        {
            BeginTextEditing(textBox);
            e.Handled = true; // Prevent this from triggering other events
        };

        textBox.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter && !textBox.IsReadOnly)
            {
                EndTextEditing(textBox);
                e.Handled = true;
            }
        };

        textBox.LostFocus += (s, e) =>
        {
            if (!textBox.IsReadOnly)
            {
                EndTextEditing(textBox);
            }

            // Clear selection when editing is done
            onEditEnd(null);
        };
    }

    /// <summary>
    /// Handle end of text editing (reset to read-only mode).
    /// </summary>
    private void BeginTextEditing(TextBox textBox)
    {
        textBox.IsReadOnly = false;
        textBox.Focusable = true;  // Enable focus for editing
        textBox.Cursor = Cursors.IBeam;
        textBox.Focus();
        textBox.SelectAll();
    }

    /// <summary>
    /// Handle end of text editing (reset to read-only mode).
    /// </summary>
    private void EndTextEditing(TextBox textBox)
    {
        textBox.IsReadOnly = true;
        textBox.Focusable = false;  // Disable focus to prevent selection
        textBox.Cursor = Cursors.Arrow;
        Keyboard.ClearFocus(); // Clear focus from textbox
    }
    #endregion

    #region Image
    public string BitmapToBase64(BitmapImage bitmap)
    {
        var encoder = new PngBitmapEncoder(); // or JpegBitmapEncoder
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using var ms = new MemoryStream();
        encoder.Save(ms);

        return Convert.ToBase64String(ms.ToArray());
    }
    public BitmapImage Base64ToBitmap(string base64)
    {
        byte[] bytes = Convert.FromBase64String(base64);

        var bitmap = new BitmapImage();
        using (var ms = new MemoryStream(bytes))
        {
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // important
            bitmap.StreamSource = ms;
            bitmap.EndInit();
            bitmap.Freeze(); // allow cross-thread usage
        }

        return bitmap;
    }
    #endregion


}
