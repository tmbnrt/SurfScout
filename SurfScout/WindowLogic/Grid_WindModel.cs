using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using SurfScout.SubWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SurfScout.WindowLogic
{
    class Grid_WindModel
    {
        MainWindow win;
        
        public Grid_WindModel(object sender, RoutedEventArgs e, MainWindow window)
        {
            this.win = window;            
        }        
    }
}
