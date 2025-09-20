using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Esri.ArcGISRuntime.Geometry;

namespace SurfScout.Models.WindModel
{
    public class WindFieldCellInterpolated
    {
        public double SpeedKnots { get; set; }
        public List<MapPoint> PolygonPoints { get; set; }
        public Color? IntensityColor { get; set; }
    }
}
