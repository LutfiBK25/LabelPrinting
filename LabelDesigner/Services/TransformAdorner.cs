using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LabelDesigner.Services;
public enum ResizeDirection
{
    TopLeft, Top, TopRight,
    Right,
    BottomLeft,Bottom, BottomRight,
    Left
}
public class TransformAdorner : Adorner
{
    private const double HANDLE_SIZE = 10;
    private const double MIN_SIZE = 20;

    private readonly VisualCollection _visuals; // Store a punch of visual objects that are rendered to the screen
    private readonly Dictionary<ResizeDirection, Thumb> _resizeThumbs = new();
    //private Thumb _thumb1, _thumb2; // to utilize draging
    private readonly Thumb _rotateThumb;

    private double _aspectRatio;
    private RotateTransform _rotateTransform;

    // Box around object
    private Rectangle _rectangle;

    public TransformAdorner(UIElement adorned) : base(adorned)
    {
        _visuals = new VisualCollection(this);

        var element = (FrameworkElement)adorned;
        _aspectRatio = element.ActualWidth / element.ActualHeight;

        // Enurse transform exists
        element.RenderTransformOrigin = new Point(0.5, 0.5);
        _rotateTransform = element.RenderTransform as RotateTransform
            ?? new RotateTransform(0);
        element.RenderTransform = _rotateTransform;

        // Create Resize Handles
        foreach(ResizeDirection dir in Enum.GetValues(typeof(ResizeDirection)))
        {
            var thumb = CreateThumb(Cursors.SizeAll);
            thumb.DragDelta += (s, e) => Resize(dir, e); //Subscribe
            _resizeThumbs[dir] = thumb;
            _visuals.Add(thumb);
        }

        // Create Rotate Handle
        _rotateThumb = CreateThumb(Cursors.Hand);
        _rotateThumb.DragDelta += Rotate; // Subscribe
        _visuals.Add(_rotateThumb);

        // Create Box around object
        _rectangle = new Rectangle() { Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4D4D4D")), StrokeThickness = 2 , StrokeDashArray = {3,2} };
        _visuals.Add(_rectangle);
    }

    private Thumb CreateThumb(Cursor cursor) => new Thumb
    {
        Width = HANDLE_SIZE,
        Height = HANDLE_SIZE,
        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4D4D4D")),
        Cursor = cursor,
    };

    // =========================
    // RESIZE LOGIC
    // =========================
    private void Resize(ResizeDirection dir, DragDeltaEventArgs e)
    {
        var el = (FrameworkElement)AdornedElement;

        bool lockRatio = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

        double dx = e.HorizontalChange;
        double dy = e.VerticalChange;

        double newWidth = el.ActualWidth;
        double newHeight = el.ActualHeight;

        double left = Canvas.GetLeft(el);
        double top = Canvas.GetTop(el);

        if (dir is ResizeDirection.Left or ResizeDirection.TopLeft or ResizeDirection.BottomLeft)
        {
            newWidth -= dx;
            left += dx;
        }

        if (dir is ResizeDirection.Right or ResizeDirection.TopRight or ResizeDirection.BottomRight)
            newWidth += dx;

        if (dir is ResizeDirection.Top or ResizeDirection.TopLeft or ResizeDirection.TopRight)
        {
            newHeight -= dy;
            top += dy;
        }

        if (dir is ResizeDirection.Bottom or ResizeDirection.BottomLeft or ResizeDirection.BottomRight)
            newHeight += dy;

        // Aspect ratio lock
        if (lockRatio)
        {
            if (Math.Abs(dx) > Math.Abs(dy))
                newHeight = newWidth / _aspectRatio;
            else
                newWidth = newHeight * _aspectRatio;
        }

        if (newWidth < MIN_SIZE || newHeight < MIN_SIZE)
            return;

        el.Width = newWidth;
        el.Height = newHeight;
        Canvas.SetLeft(el, left);
        Canvas.SetTop(el, top);
    }

    // =========================
    // ROTATION
    // =========================
    private void Rotate(object sender, DragDeltaEventArgs e)
    {
        var el = (FrameworkElement)AdornedElement;

        Point center = el.TranslatePoint(
            new Point(el.Width / 2, el.Height / 2),
            Application.Current.MainWindow);

        Point mouse = Mouse.GetPosition(Application.Current.MainWindow);

        double angle = Math.Atan2(mouse.Y - center.Y, mouse.X - center.X) * 180 / Math.PI;
        _rotateTransform.Angle = angle + 90;
    }

    // =========================
    // ADORNER LAYOUT
    // =========================
    protected override Size ArrangeOverride(Size finalSize)
    {
        var el = (FrameworkElement)AdornedElement;
        double w = el.ActualWidth;
        double h = el.ActualHeight;
        double hs = HANDLE_SIZE / 2;

        Arrange(ResizeDirection.TopLeft, -5, -5);
        Arrange(ResizeDirection.Top, w / 2, -5);
        Arrange(ResizeDirection.TopRight, w + 5, -5);
        Arrange(ResizeDirection.Right, w + 5, h / 2);
        Arrange(ResizeDirection.BottomRight, w + 5, h + 5);
        Arrange(ResizeDirection.Bottom, w / 2, h + 5);
        Arrange(ResizeDirection.BottomLeft, -5, h + 5);
        Arrange(ResizeDirection.Left, - 5, h / 2);

        _rotateThumb.Arrange(new Rect(w / 2 - hs, -30, HANDLE_SIZE, HANDLE_SIZE));

        // Object Box
        _rectangle.Arrange(new Rect(-2.5, -2.5, w + 5,h + 5));

        return finalSize;
    }

    private void Arrange(ResizeDirection dir, double x, double y)
    {
        _resizeThumbs[dir].Arrange(
            new Rect(x - HANDLE_SIZE / 2, y - HANDLE_SIZE / 2, HANDLE_SIZE, HANDLE_SIZE));
    }

    protected override int VisualChildrenCount => _visuals.Count;
    protected override Visual GetVisualChild(int index) => _visuals[index];
}
