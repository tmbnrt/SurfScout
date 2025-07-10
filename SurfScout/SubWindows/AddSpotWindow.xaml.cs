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

namespace SurfScout.SubWindows
{
    /// <summary>
    /// Logic for AddSpotWindow.xaml
    /// </summary>
    public partial class AddSpotWindow : Window
    {
        public string SpotName {  get; private set; }

        public AddSpotWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SpotName = NameTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }
    }
}
