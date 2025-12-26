using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LabelPrinting.Domain.Entities.Label.Elements;
namespace LabelDesigner.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var dlg = new NewLabelWindow();
            if (dlg.ShowDialog() == true)
            {
                SetLabelSize(dlg.LabelWidthIn, dlg.LabelHeightIn, dlg.Dpi);
            }
            else
            {
                Close();
            }
        }

        // Canvas Variables
        private int _dpi = 203;

        // Dragging Variables
        private Point _startPoint;
        private bool _isDragging;


        // Set canvas size during initialization
        private void SetLabelSize(double widthInches, double heightInches, int dpi)
        {
            _dpi = dpi;

            double widthPx = widthInches * dpi;
            double heightPx = heightInches * dpi;

            LabelCanvas.Width = widthPx;
            LabelCanvas.Height = heightPx;

        }

        // Set canvas size based on input width and height in inches
        private void ApplySize_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(WidthBox.Text, out double widthInches))
                return;

            if (!double.TryParse(HeightBox.Text, out double heightInches))
                return;

            double widthPx = widthInches * _dpi; // Convert inches to pixels
            double heightPx = heightInches * _dpi;

            LabelCanvas.Width = widthPx; // Set canvas size
            LabelCanvas.Height = heightPx;
        }



        private void MakeDraggable(UIElement element)
        {
            element.MouseLeftButtonDown += (s, e) =>
            {
                _isDragging = true;
                _startPoint = e.GetPosition(LabelCanvas);
                element.CaptureMouse();
            };

            element.MouseMove += (s, e) =>
            {
                if (!_isDragging) return;

                var position = e.GetPosition(LabelCanvas);

                double offsetX = position.X - _startPoint.X;
                double offsetY = position.Y - _startPoint.Y;

                // Calculate new position
                double newLeft = Canvas.GetLeft(element) + offsetX;
                double newTop = Canvas.GetTop(element) + offsetY;


                // Clamp inside canvas
                double elementWidth = (element as FrameworkElement)?.ActualWidth ?? 0;
                double elementHeight = (element as FrameworkElement)?.ActualHeight ?? 0;

                newLeft = Math.Max(0, Math.Min(newLeft, LabelCanvas.Width - elementWidth));
                newTop = Math.Max(0, Math.Min(newTop, LabelCanvas.Height - elementHeight));

                Canvas.SetLeft(element, newLeft);
                Canvas.SetTop(element, newTop);


                _startPoint = position;
            };

            element.MouseLeftButtonUp += (s, e) =>
            {
                _isDragging = false;
                element.ReleaseMouseCapture();
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

            var textBlock = new TextBlock
            {
                Text = element.Text,
                FontSize = 24,
                Background = Brushes.Transparent
            };

            Canvas.SetLeft(textBlock, element.X);
            Canvas.SetTop(textBlock, element.Y);

            MakeDraggable(textBlock);

            LabelCanvas.Children.Add(textBlock);
        }




    }
}