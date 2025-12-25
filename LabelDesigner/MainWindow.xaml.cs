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
namespace LabelDesigner
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

        private int _dpi = 203;

        private const int GridSize = 10;

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


        private Point _startPoint;
        private bool _isDragging;

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

                Canvas.SetLeft(element, Canvas.GetLeft(element) + offsetX);
                Canvas.SetTop(element, Canvas.GetTop(element) + offsetY);

                _startPoint = position;
            };

            element.MouseLeftButtonUp += (s, e) =>
            {
                _isDragging = false;
                element.ReleaseMouseCapture();
            };
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

        private double Snap(double value)
        {
            return Math.Round(value / GridSize) * GridSize;
        }

        UIElement dragged;
        Point start;

        private void Element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            dragged = sender as UIElement;
            start = e.GetPosition(LabelCanvas);
            dragged.CaptureMouse();
        }

        private void Element_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragged == null) return;

            Point pos = e.GetPosition(LabelCanvas);

            double left = Snap(pos.X - start.X + Canvas.GetLeft(dragged));
            double top = Snap(pos.Y - start.Y + Canvas.GetTop(dragged));

            // 🚫 Prevent leaving canvas
            left = Math.Max(0, Math.Min(left, LabelCanvas.Width - ((FrameworkElement)dragged).ActualWidth));
            top = Math.Max(0, Math.Min(top, LabelCanvas.Height - ((FrameworkElement)dragged).ActualHeight));

            Canvas.SetLeft(dragged, left);
            Canvas.SetTop(dragged, top);
        }

        private void Element_MouseUp(object sender, MouseButtonEventArgs e)
        {
            dragged?.ReleaseMouseCapture();
            dragged = null;
        }

        private void DrawRulers()
        {
            TopRuler.Children.Clear();
            LeftRuler.Children.Clear();

            for (int x = 0; x < LabelCanvas.Width; x += GridSize)
            {
                TopRuler.Children.Add(new Line
                {
                    X1 = x,
                    Y1 = 30,
                    X2 = x,
                    Y2 = x % 50 == 0 ? 10 : 20,
                    Stroke = Brushes.Black
                });
            }

            for (int y = 0; y < LabelCanvas.Height; y += GridSize)
            {
                LeftRuler.Children.Add(new Line
                {
                    X1 = 30,
                    Y1 = y,
                    X2 = y % 50 == 0 ? 10 : 20,
                    Y2 = y,
                    Stroke = Brushes.Black
                });
            }
        }

        private void SetLabelSize(double widthInches, double heightInches, int dpi)
        {
            _dpi = dpi;

            double widthPx = widthInches * dpi;
            double heightPx = heightInches * dpi;

            LabelCanvas.Width = widthPx;
            LabelCanvas.Height = heightPx;

            DrawRulers();
        }
    }
}