using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LabelDesigner.Services;
public enum ResizeDirection
{
    TopLeft, Top, TopRight,
    Right,
    BottomLeft,Bottom, BottomRight,
    Left
}
public class ResizeAdorner : Adorner
{
    private const double HANDLE_SIZE = 10;
    private const double MIN_SIZE = 20;

    private readonly VisualCollection _visuals; // Store a punch of visual objects that are rendered to the screen
    private readonly Dictionary<ResizeDirection, Thumb> _resizeThumbs = new();
    //private Thumb _thumb1, _thumb2; // to utilize draging
    private readonly Thumb _rotateThumb;

    private double _aspectRatio;
    private RotateTransform _rotateTransform;

    public ResizeAdorner(UIElement adorned) : base(adorned)
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
    }

    private Thumb CreateThumb(Cursor cursor) => new Thumb
    {
        Width = HANDLE_SIZE,
        Height = HANDLE_SIZE,
        Background = Brushes.Coral,
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

        double newWidth = el.Width;
        double newHeight = el.Height;

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

        Arrange(ResizeDirection.TopLeft, 0, 0);
        Arrange(ResizeDirection.Top, w / 2, 0);
        Arrange(ResizeDirection.TopRight, w, 0);
        Arrange(ResizeDirection.Right, w, h / 2);
        Arrange(ResizeDirection.BottomRight, w, h);
        Arrange(ResizeDirection.Bottom, w / 2, h);
        Arrange(ResizeDirection.BottomLeft, 0, h);
        Arrange(ResizeDirection.Left, 0, h / 2);

        _rotateThumb.Arrange(new Rect(w / 2 - hs, -30, HANDLE_SIZE, HANDLE_SIZE));

        return finalSize;
    }

    private void Arrange(ResizeDirection dir, double x, double y)
    {
        _resizeThumbs[dir].Arrange(
            new Rect(x - HANDLE_SIZE / 2, y - HANDLE_SIZE / 2, HANDLE_SIZE, HANDLE_SIZE));
    }

    protected override int VisualChildrenCount => _visuals.Count;
    protected override Visual GetVisualChild(int index) => _visuals[index];

    //public ResizeAdorner(UIElement adornedElement) : base(adornedElement)
    //{
    //    // Implment Rendering for Adorner
    //    _visuals = new VisualCollection(this);

    //    // Creating the thumbs
    //    _thumb1 = new Thumb() { Background = Brushes.Coral, Height = 10, Width = 10 };
    //    _thumb2 = new Thumb() { Background = Brushes.Coral, Height = 10, Width = 10 };

    //    //Subscribe to event class
    //    _thumb1.DragDelta += Thumb1_DragDelta;
    //    _thumb2.DragDelta += Thumb2_DragDelta;


    //    // Adding them to the adorner collection
    //    _visuals.Add(_thumb1);
    //    _visuals.Add(_thumb2);

    //}


    //private void Thumb1_DragDelta(object sender, DragDeltaEventArgs e)
    //{
    //    // we are casting to a framework to access the properties
    //    var element = (FrameworkElement)AdornedElement;
    //    // We go small as long as we stop at zero or the program crash
    //    element.Height = element.Height - e.VerticalChange < 0 ? 0 : element.Height - e.VerticalChange;
    //    element.Width = element.Width - e.HorizontalChange < 0 ? 0 : element.Width - e.HorizontalChange;

    //    double left = Canvas.GetLeft(element);
    //    double top = Canvas.GetTop(element);

    //    Canvas.SetLeft(element, left + e.HorizontalChange);
    //    Canvas.SetTop(element, top + e.VerticalChange);
    //}

    //private void Thumb2_DragDelta(object sender, DragDeltaEventArgs e)
    //{
    //    // we are casting to a framework to access the properties
    //    var element = (FrameworkElement)AdornedElement;
    //    // We go small as long as we stop at zero or the program crash
    //    element.Height = element.Height + e.VerticalChange < 0 ? 0 : element.Height + e.VerticalChange;
    //    element.Width = element.Width + e.HorizontalChange < 0 ? 0 : element.Width + e.HorizontalChange;

    //}


    //// This method gets called when the adorner is rendered to the screen
    //protected override Visual GetVisualChild(int index)
    //{
    //    return _visuals[index];
    //}

    //// Return the number of the visuals
    //protected override int VisualChildrenCount => _visuals.Count;

    ////Arrange the visuals
    //protected override Size ArrangeOverride(Size finalSize)
    //{
    //    // Arrange it relative to the UI Element
    //    _thumb1.Arrange(new Rect(0, 0, 10, 10));
    //    _thumb2.Arrange(new Rect(AdornedElement.DesiredSize.Width,AdornedElement.DesiredSize.Height, 10, 10));


    //    return base.ArrangeOverride(finalSize);
    //}
}
