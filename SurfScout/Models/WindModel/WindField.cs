using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;

namespace SurfScout.Models.WindModel
{
    // Class representing the wind data for datapoints (in list) for a specific time (hourly)
    public class WindField
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly Timestamp { get; set; }
        public int SessionId { get; set; }
        public ICollection<WindFieldPoint> Points { get; set; }
    }
}
