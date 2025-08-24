using SurfScout.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Logic for AddSessionWindow.xaml
    /// </summary>
    public partial class AddSessionWindow : Window
    {
        public DateOnly date { get; private set; }
        public TimeOnly startTime { get; private set; }
        public TimeOnly endTime { get; private set; }
        public string waveHeight { get; private set; }
        public int rating { get; private set; }
        public double sailSize { get; private set; }

        public AddSessionWindow()
        {
            InitializeComponent();

            label_SportMode.Content = UserSession.SelectedSportMode;

            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

            datePicker.SelectedDate = DateTime.Today;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void SaveSession_Click(object sender, RoutedEventArgs e)
        {
            if (datePicker.SelectedDate == null ||
                comboStartTime.SelectedItem == null ||
                comboEndTime.SelectedItem == null ||
                comboWaveHeight.SelectedItem == null ||
                string.IsNullOrWhiteSpace(txtSailSize.Text))
            {
                MessageBox.Show("Please fill in all fields before saving.", "Missing Data", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            this.date = DateOnly.FromDateTime(datePicker.SelectedDate ?? DateTime.Today);
            this.waveHeight = comboWaveHeight.Text;
            this.startTime = TimeOnly.Parse((comboStartTime.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "08:00");
            this.endTime = TimeOnly.Parse((comboEndTime.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "08:00");
            this.rating = (int)sliderRating.Value;

            string input = txtSailSize.Text.Replace(",", ".");
            if (!double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double sailSize))
            {
                MessageBox.Show("Invalid sail size format. Please enter a number like 4.9 or 4,3.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            this.sailSize = sailSize;

            if (startTime >= endTime)
            {
                MessageBox.Show("Start time must be before end time.", "Time Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;

            this.Close();
            return;
        }
    }
}
