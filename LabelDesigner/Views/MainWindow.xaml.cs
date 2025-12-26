using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LabelPrinting.Domain.Entities.Label.Elements;
namespace LabelDesigner.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Label Size Variables
        private double _labelWidthIn;
        private double _labelHeightIn;

        // Selected Element
        private UIElement? _selectedElement;



        public MainWindow()
        {
            InitializeComponent();

            var dlg = new NewLabelWindow();
            if (dlg.ShowDialog() == true)
            {
                // Get label size from dialog
                _labelWidthIn = dlg.LabelWidthIn;
                _labelHeightIn = dlg.LabelHeightIn;
                // Update UI Label Size Boxes
                WidthBox.Text = _labelWidthIn.ToString();
                HeightBox.Text = _labelHeightIn.ToString();

                SetLabelSize(dlg.LabelWidthIn, dlg.LabelHeightIn);
            }
            else
            {
                Close();
            }

            // Make sure the window can capture key events
            this.KeyDown += MainWindow_KeyDown;
            this.Focusable = true;
            this.Focus(); // set focus to the window
            LabelCanvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
        }

        // Set canvas size during initialization
        private void SetLabelSize(double widthInches, double heightInches)
        {
            // You can define a scale factor for the designer view, e.g. 100 pixels per inch
            const double scale = 100;

            LabelCanvas.Width = widthInches * scale;
            LabelCanvas.Height = heightInches * scale;
        }


        // Set canvas size based on input width and height in inches
        private void ApplySize_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(WidthBox.Text, out double widthInches)) return;
            if (!double.TryParse(HeightBox.Text, out double heightInches)) return;

            _labelWidthIn = widthInches;
            _labelHeightIn = heightInches;

            SetLabelSize(_labelWidthIn, _labelHeightIn);

            // Ensure elements stay inside
            ClampElementsToCanvas();
        }

        // Ensure elements stay within canvas bounds after resizing
        private void ClampElementsToCanvas()
        {
            foreach (UIElement element in LabelCanvas.Children)
            {
                if (element is FrameworkElement fe)
                {
                    double left = Canvas.GetLeft(fe);
                    double top = Canvas.GetTop(fe);

                    // Handle NaN values
                    if (double.IsNaN(left)) left = 0;
                    if (double.IsNaN(top)) top = 0;

                    // Clamp positions
                    left = Math.Max(0, Math.Min(left, LabelCanvas.Width - fe.ActualWidth));
                    top = Math.Max(0, Math.Min(top, LabelCanvas.Height - fe.ActualHeight));

                    Canvas.SetLeft(fe, left);
                    Canvas.SetTop(fe, top);
                }
            }
        }

        // Canvas method:
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Only deselect if clicking directly on canvas (not on a child element)
            if (e.Source == LabelCanvas)
            {
                if (_selectedElement != null && _selectedElement is TextBox tb)
                {
                    tb.IsReadOnly = true;
                    tb.Focusable = false;  // Disable focus to prevent selection
                    tb.Cursor = Cursors.Arrow;
                    Keyboard.ClearFocus(); // Clear focus from textbox
                }
                _selectedElement = null;
                HighlightSelectedElement(null);
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && _selectedElement != null)
            {
                LabelCanvas.Children.Remove(_selectedElement);
                _selectedElement = null;
            }
        }

        // Dragging Element
        private void MakeDraggable(UIElement element)
        {
            Point startPoint = default;
            element.MouseLeftButtonDown += (s, e) =>
            {
                // Don't start dragging if the textbox is in edit mode
                if (element is TextBox tb && !tb.IsReadOnly)
                    return;

                startPoint = e.GetPosition(LabelCanvas);
                element.CaptureMouse();
                _selectedElement = element;
                HighlightSelectedElement(element);
                e.Handled = true;
            };

            element.MouseLeftButtonUp += (s, e) =>
            {
                element.ReleaseMouseCapture();
                e.Handled = true;
            };

            element.MouseMove += (s, e) =>
            {
                // Check if THIS element has mouse capture (is being dragged)
                if (!element.IsMouseCaptured) return;

                // Don't drag if in edit mode
                if (element is TextBox tb && !tb.IsReadOnly)
                    return;

                var position = e.GetPosition(LabelCanvas);
                double offsetX = position.X - startPoint.X;
                double offsetY = position.Y - startPoint.Y;

                // Get current position, handling NaN
                double currentLeft = Canvas.GetLeft(element);
                double currentTop = Canvas.GetTop(element);

                if (double.IsNaN(currentLeft)) currentLeft = 0;
                if (double.IsNaN(currentTop)) currentTop = 0;

                double newLeft = currentLeft + offsetX;
                double newTop = currentTop + offsetY;

                // Clamp inside canvas
                if (element is FrameworkElement fe)
                {
                    newLeft = Math.Max(0, Math.Min(newLeft, LabelCanvas.Width - fe.ActualWidth));
                    newTop = Math.Max(0, Math.Min(newTop, LabelCanvas.Height - fe.ActualHeight));
                }

                Canvas.SetLeft(element, newLeft);
                Canvas.SetTop(element, newTop);
                startPoint = position;
                e.Handled = true;
            };
        }
        // Add Text Element
        private void AddText_Click(object sender, RoutedEventArgs e)
        {
            var element = new LabelTextElement
            {
                Text = "New Text",
                X = 50,
                Y = 50
            };

            var textBox = new TextBox
            {
                Text = element.Text,
                FontSize = 24,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                IsReadOnly = true, // start in read-only mode
                IsHitTestVisible = true,       // make sure it can receive mouse events
                Padding = new Thickness(2),     // optional: makes it easier to grab
                Cursor = Cursors.Arrow, // Set initial cursor to arrow
                Focusable = false  // Add this - prevents text selection when read-only
            };

            Canvas.SetLeft(textBox, element.X);
            Canvas.SetTop(textBox, element.Y);

            MakeDraggable(textBox);
            MakeEditable(textBox);
            LabelCanvas.Children.Add(textBox);        
        }

        // Make TextBox Editable on Double Click
        private void MakeEditable(TextBox textBox)
        {
            // Set initial cursor to arrow (for dragging)
            textBox.Cursor = Cursors.Arrow;

            textBox.MouseDoubleClick += (s, e) =>
            {
                textBox.IsReadOnly = false;
                textBox.Focusable = true;  // Enable focus for editing
                textBox.Cursor = Cursors.IBeam;
                textBox.Focus();
                textBox.SelectAll();
                e.Handled = true; // Prevent this from triggering other events
            };

            textBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && !textBox.IsReadOnly)
                {
                    textBox.IsReadOnly = true;
                    textBox.Focusable = false;  // Disable focus to prevent selection
                    textBox.Cursor = Cursors.Arrow;
                    Keyboard.ClearFocus(); // Clear focus from textbox
                    e.Handled = true;
                }
            };

            textBox.LostFocus += (s, e) =>
            {
                textBox.IsReadOnly = true;
                textBox.Focusable = false;  // Disable focus to prevent selection
                textBox.Cursor = Cursors.Arrow;

                // Clear selection when editing is done
                if (_selectedElement == textBox)
                {
                    _selectedElement = null;
                    HighlightSelectedElement(null);
                }
            };
        }


        // Highlight selected element
        private void HighlightSelectedElement(UIElement? element)
        {
            // Clear all highlights
            foreach (UIElement child in LabelCanvas.Children)
            {
                if (child is TextBox tb)
                {
                    tb.BorderBrush = null;
                    tb.BorderThickness = new Thickness(0);
                }
            }

            // Highlight selected element
            if (element is TextBox selected)
            {
                selected.BorderBrush = Brushes.Blue;
                selected.BorderThickness = new Thickness(2);
            }
        }
    }
}