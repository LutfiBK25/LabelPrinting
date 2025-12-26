using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LabelDesigner
{
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
}
