using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;

namespace SurfScout.Models.WindModel
{
    // Class representing the time data set in list format
    public class WindFetchDataSet
    {
        // TO DO: navigation property to specific session is needed here!
        // ...
        public List<WindFetchDataSetPoint> DataPoint { get; set; } = new();

    }
}
