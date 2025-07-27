using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace SurfScout.Functions
{
    public static class UI_Helpers
    {
        public static void SetButtonState(Button btn, string command)
        {
            if (btn == null || string.IsNullOrWhiteSpace(command)) return;

            if (command.ToLower() == "enable")
            {
                btn.IsEnabled = true;
                btn.ClearValue(Button.BackgroundProperty);
                btn.ClearValue(Button.ForegroundProperty);
                btn.ClearValue(Button.BorderBrushProperty);
                btn.ClearValue(Button.OpacityProperty);
            }
            else if (command.ToLower() == "disable")
            {
                btn.IsEnabled = false;
                btn.Background = new SolidColorBrush(Color.FromRgb(224, 224, 224));
                btn.Foreground = Brushes.Gray;
                btn.BorderBrush = new SolidColorBrush(Color.FromRgb(170, 170, 170));
                btn.Opacity = 0.6;
            }
        }

        public static List<string> GenerateStars(int rating)
        {
            return Enumerable.Repeat("★", rating).ToList();
        }
    }
}
