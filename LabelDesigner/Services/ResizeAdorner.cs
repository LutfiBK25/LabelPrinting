using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace LabelDesigner.Services;

public class ResizeAdorner : Adorner
{
    private VisualCollection _adornerVisuals; // Store a punch of visual objects that are rendered to the screen
    private Thumb _thumb1, _thumb2; // to utilize draging
    public ResizeAdorner(UIElement adornedElement) : base(adornedElement)
    {
        // Implment Rendering for Adorner
        _adornerVisuals = new VisualCollection(this);

        // Creating the thumbs
        _thumb1 = new Thumb() { Background = Brushes.Coral, Height=10, Width=10 }  ;
        _thumb2 = new Thumb() { Background = Brushes.Coral, Height = 10, Width = 10 };

        //Subscribe to event class
        _thumb1.DragDelta += Thumb1_DragDelta;
        _thumb2.DragDelta += Thumb2_DragDelta;


        // Adding them to the adorner collection
        _adornerVisuals.Add(_thumb1);
        _adornerVisuals.Add(_thumb2);

    }

    private void Thumb1_DragDelta(object sender, DragDeltaEventArgs e)
    {
        // we are casting to a framework to access the properties
        var element = (FrameworkElement)AdornedElement;
        // We go small as long as we stop at zero or the program crash
        element.Height = element.Height - e.VerticalChange < 0 ? 0 : element.Height - e.VerticalChange;
        element.Width = element.Width - e.HorizontalChange < 0 ? 0 : element.Width - e.HorizontalChange;

        double left = Canvas.GetLeft(element);
        double top = Canvas.GetTop(element);

        Canvas.SetLeft(element, left + e.HorizontalChange);
        Canvas.SetTop(element, top + e.VerticalChange);
    }

    private void Thumb2_DragDelta(object sender, DragDeltaEventArgs e)
    {
        // we are casting to a framework to access the properties
        var element = (FrameworkElement)AdornedElement;
        // We go small as long as we stop at zero or the program crash
        element.Height = element.Height + e.VerticalChange < 0 ? 0 : element.Height + e.VerticalChange;
        element.Width = element.Width + e.HorizontalChange < 0 ? 0 : element.Width + e.HorizontalChange;

    }


    // This method gets called when the adorner is rendered to the screen
    protected override Visual GetVisualChild(int index)
    {
        return _adornerVisuals[index];
    }

    // Return the number of the visuals
    protected override int VisualChildrenCount => _adornerVisuals.Count;

    //Arrange the visuals
    protected override Size ArrangeOverride(Size finalSize)
    {
        // Arrange it relative to the UI Element
        _thumb1.Arrange(new Rect(0, 0, 10, 10));
        _thumb2.Arrange(new Rect(AdornedElement.DesiredSize.Width,AdornedElement.DesiredSize.Height, 10, 10));


        return base.ArrangeOverride(finalSize);
    }
}
