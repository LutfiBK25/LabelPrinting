using LabelDesigner.Services;
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
        // You can define a scale factor for the designer view, e.g. 100 pixels per inch
        private const double scale = 100;

        // Store the current label being edited
        private Label? _currentLabel;

        // Store domain entities alongside UI elements
        private Dictionary<UIElement, LabelElement> _elementMapping = new Dictionary<UIElement, LabelElement>();

        // Canvas element service for managing interactions
        private CanvasElementService _canvasElementService;

        // Label Size Variables
        private double _labelWidthIn;
        private double _labelHeightIn;

        // Selected Element
        private UIElement? _selectedElement;



        public MainWindow()
        {
            InitializeComponent();

            // Initialize canvas element service
            _canvasElementService = new CanvasElementService(LabelCanvas, OnElementSelectionChanged);

            // Show New Label Dialog on Startup
            var dlg = new NewLabelWindow();
            
            if (dlg.ShowDialog() == true)
            {
                // Create new label - ADD THIS
                _currentLabel = new Label
                {
                    Id = Guid.NewGuid(),
                    Name = "Untitled Label",
                    LabelWidthInches = dlg.LabelWidthIn,
                    LabelHeightInches = dlg.LabelHeightIn
                };

                // Initialize label size variables for Label size in UI
                // Get label size from dialog
                _labelWidthIn = dlg.LabelWidthIn;
                _labelHeightIn = dlg.LabelHeightIn;
                // Update UI Label Size Boxes
                WidthBox.Text = _labelWidthIn.ToString();
                HeightBox.Text = _labelHeightIn.ToString();

                // Set canvas size
                SetLabelSize(dlg.LabelWidthIn, dlg.LabelHeightIn);
            }
            else
            {
                Close();
            }

            // Setup window for keyboard events
            // so it can receive key events
            Focusable = true;
            // Set initial focus to the window
            Focus();
            // Subscribe to KeyDown event
            KeyDown += MainWindow_KeyDown;
            MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
        }

        /// <summary>
        /// Callback when element selection changes.
        /// </summary>
        private void OnElementSelectionChanged(UIElement? element)
        {
            _selectedElement = element;
            _canvasElementService.HighlightSelectedElement(element);
        }

        // Set canvas size during initialization
        private void SetLabelSize(double widthInches, double heightInches)
        {
            LabelCanvas.Width = widthInches * scale;
            LabelCanvas.Height = heightInches * scale;
        }

        // Canvas method:
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && _selectedElement != null)
            {
                LabelCanvas.Children.Remove(_selectedElement);
                if (_elementMapping.TryGetValue(_selectedElement, out _))
                {
                    _elementMapping.Remove(_selectedElement);
                }
                _selectedElement = null;
            }
        }

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
                _canvasElementService.HighlightSelectedElement(null);
            }
        }

        // Set canvas size based on input width and height in Label Size section
        private void ApplySize_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(WidthBox.Text, out double widthInches)) return;
            if (!double.TryParse(HeightBox.Text, out double heightInches)) return;

            _labelWidthIn = widthInches;
            _labelHeightIn = heightInches;

            SetLabelSize(_labelWidthIn, _labelHeightIn);

            // Ensure elements stay inside
            _canvasElementService.ClampElementsToCanvas();
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
                ElementWidth = 0,
                ElementHeight = 0
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

            _canvasElementService.MakeDraggable(textBox);
            _canvasElementService.MakeTextBoxEditable(textBox, _ => _canvasElementService.HighlightSelectedElement(null));
            LabelCanvas.Children.Add(textBox);

            // Store the mapping
            _elementMapping[textBox] = domainElement;
        }

        // Application Window Bar
        // Draging using Border
        private void Border_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        // Minimizing Application
        private void Button_Minimize_Click (object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        // Maximize and Normal Window State
        private void Button_WindowState_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow.WindowState != WindowState.Maximized)
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            else
                Application.Current.MainWindow.WindowState = WindowState.Normal;
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // File Menu Methods

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
                    LabelWidthInches = _labelWidthIn,
                    LabelHeightInches = _labelHeightIn
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
                                domainElement.ElementWidth = fe.ActualWidth;
                                domainElement.ElementHeight = fe.ActualHeight;
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
                    _currentLabel.LabelWidthInches = _labelWidthIn;
                    _currentLabel.LabelHeightInches = _labelHeightIn;

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
                    _labelWidthIn = label.LabelWidthInches;
                    _labelHeightIn = label.LabelHeightInches;
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

        // Helper method to create UI element from domain entity (Loading Design)
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

                _canvasElementService.MakeDraggable(textBox);
                _canvasElementService.MakeTextBoxEditable(textBox, _ => _canvasElementService.HighlightSelectedElement(null));
                LabelCanvas.Children.Add(textBox);

                // Store the mapping
                _elementMapping[textBox] = textElement;
            }
            // ToDo: Add more element types here as needed (images, barcodes, etc.)
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
                        LabelWidthInches = dlg.LabelWidthIn,
                        LabelHeightInches = dlg.LabelHeightIn
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

    }
}