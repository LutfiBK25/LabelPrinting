using System.Windows;

namespace LabelDesigner;

/// <summary>
/// Interaction logic for NewLabelWindow.xaml
/// </summary>
public partial class NewLabelWindow : Window
{
    public NewLabelWindow()
    {
        InitializeComponent();
    }

    public double LabelWidthIn { get; private set; }
    public double LabelHeightIn { get; private set; }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        LabelWidthIn = double.Parse(WidthBox.Text);
        LabelHeightIn = double.Parse(HeightBox.Text);
        DialogResult = true;
    }
}
