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
namespace LabelDesigner.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Store the current label being edited
        private Label _currentLabel;

        // Store domain entities alongside UI elements
        private Dictionary<UIElement, LabelElement> _elementMapping = new Dictionary<UIElement, LabelElement>();


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
                // Create new label - ADD THIS
                _currentLabel = new Label
                {
                    Id = Guid.NewGuid(),
                    Name = "Untitled Label",
                    WidthInches = dlg.LabelWidthIn,
                    HeightInches = dlg.LabelHeightIn
                };
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
        // Update AddText_Click to use the mapping
        private void AddText_Click(object sender, RoutedEventArgs e)
        {
            var domainElement = new LabelTextElement
            {
                Id = Guid.NewGuid(),
                Text = "New Text",
                X = 50,
                Y = 50,
                FontSize = 24,
                Width = 0,
                Height = 0
            };

            var textBox = new TextBox
            {
                Text = domainElement.Text,
                FontSize = domainElement.FontSize,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                IsReadOnly = true,
                IsHitTestVisible = true,
                Padding = new Thickness(2),
                Cursor = Cursors.Arrow,
                Focusable = false
            };

            Canvas.SetLeft(textBox, domainElement.X);
            Canvas.SetTop(textBox, domainElement.Y);

            MakeDraggable(textBox);
            MakeEditable(textBox);
            LabelCanvas.Children.Add(textBox);

            // Store the mapping
            _elementMapping[textBox] = domainElement;
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

        // Menu Methods

        // Save Design to JSON
        private void SaveDesign_Click(object sender, RoutedEventArgs e)
        {
            // Safety check
            if (_currentLabel == null)
            {
                _currentLabel = new Label
                {
                    Id = Guid.NewGuid(),
                    Name = "Untitled Label",
                    WidthInches = _labelWidthIn,
                    HeightInches = _labelHeightIn
                };
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Label Design Files (*.lbl)|*.lbl|All Files (*.*)|*.*",
                DefaultExt = ".lbl",
                Title = "Save Label Design",
                FileName = _currentLabel.Name
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    // Update label name from filename
                    _currentLabel.Name = Path.GetFileNameWithoutExtension(saveDialog.FileName);

                    // Clear existing elements
                    _currentLabel.Elements.Clear();

                    // Collect all elements from canvas
                    foreach (UIElement uiElement in LabelCanvas.Children)
                    {
                        if (_elementMapping.TryGetValue(uiElement, out var domainElement))
                        {
                            // Update position from UI
                            double x = Canvas.GetLeft(uiElement);
                            double y = Canvas.GetTop(uiElement);

                            if (double.IsNaN(x)) x = 0;
                            if (double.IsNaN(y)) y = 0;

                            domainElement.X = x;
                            domainElement.Y = y;

                            // Update size from UI
                            if (uiElement is FrameworkElement fe)
                            {
                                domainElement.Width = fe.ActualWidth;
                                domainElement.Height = fe.ActualHeight;
                            }

                            // Update text content if it's a TextBox
                            if (uiElement is TextBox textBox && domainElement is LabelTextElement textElement)
                            {
                                textElement.Text = textBox.Text;
                                textElement.FontSize = textBox.FontSize;
                            }

                            // Convert to serializable format
                            _currentLabel.Elements.Add(SerializableLabelElement.FromDomain(domainElement));
                        }
                    }

                    // Update label dimensions
                    _currentLabel.WidthInches = _labelWidthIn;
                    _currentLabel.HeightInches = _labelHeightIn;

                    // Serialize to JSON
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(_currentLabel, options);
                    File.WriteAllText(saveDialog.FileName, json);

                    MessageBox.Show("Design saved successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving design: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Load Design from JSON
        private void LoadDesign_Click(object sender, RoutedEventArgs e)
        {
            // Open file dialog
            var openDialog = new OpenFileDialog
            {
                Filter = "Label Design Files (*.lbl)|*.lbl|All Files (*.*)|*.*",
                Title = "Load Label Design"
            };

            // Show dialog
            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    // Read and deserialize JSON
                    string json = File.ReadAllText(openDialog.FileName);
                    var label = JsonSerializer.Deserialize<Label>(json);

                    if (label == null)
                    {
                        MessageBox.Show("Invalid design file.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Clear current canvas
                    LabelCanvas.Children.Clear();
                    _elementMapping.Clear();
                    _selectedElement = null;

                    // Set current label
                    _currentLabel = label;

                    // Set label size
                    _labelWidthIn = label.WidthInches;
                    _labelHeightIn = label.HeightInches;
                    WidthBox.Text = _labelWidthIn.ToString();
                    HeightBox.Text = _labelHeightIn.ToString();
                    SetLabelSize(_labelWidthIn, _labelHeightIn);

                    // Update window title with label name
                    this.Title = $"Label Designer - {label.Name}";

                    // Recreate elements
                    foreach (var serializableElement in label.Elements)
                    {
                        var domainElement = serializableElement.ToDomain();
                        CreateUIElement(domainElement);
                    }

                    MessageBox.Show("Design loaded successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading design: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // New Design (Clear Canvas)
        private void NewDesign_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "This will clear the current design. Are you sure?",
                "New Design",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var dlg = new NewLabelWindow();
                if (dlg.ShowDialog() == true)
                {
                    // Clear canvas and mappings
                    LabelCanvas.Children.Clear();
                    _elementMapping.Clear();
                    _selectedElement = null;

                    // Create new label
                    _currentLabel = new Label
                    {
                        Id = Guid.NewGuid(),
                        Name = "Untitled Label",
                        WidthInches = dlg.LabelWidthIn,
                        HeightInches = dlg.LabelHeightIn
                    };

                    _labelWidthIn = dlg.LabelWidthIn;
                    _labelHeightIn = dlg.LabelHeightIn;
                    WidthBox.Text = _labelWidthIn.ToString();
                    HeightBox.Text = _labelHeightIn.ToString();

                    SetLabelSize(dlg.LabelWidthIn, dlg.LabelHeightIn);

                    // Reset window title
                    this.Title = "Label Designer";
                }
            }
        }


        // Helper method to create UI element from domain entity
        private void CreateUIElement(LabelElement domainElement)
        {
            if (domainElement is LabelTextElement textElement)
            {
                var textBox = new TextBox
                {
                    Text = textElement.Text,
                    FontSize = textElement.FontSize,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    IsReadOnly = true,
                    IsHitTestVisible = true,
                    Padding = new Thickness(2),
                    Cursor = Cursors.Arrow,
                    Focusable = false
                };

                Canvas.SetLeft(textBox, textElement.X);
                Canvas.SetTop(textBox, textElement.Y);

                MakeDraggable(textBox);
                MakeEditable(textBox);
                LabelCanvas.Children.Add(textBox);

                // Store the mapping
                _elementMapping[textBox] = textElement;
            }
            // ToDo: Add more element types here as needed (images, barcodes, etc.)
        }
    }
}