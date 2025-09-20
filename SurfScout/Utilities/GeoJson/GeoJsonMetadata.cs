using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfScout.Utilities.GeoJson
{
    public class GeoJsonMetadata
    {
        public string Date { get; set; }
        public string Timestamp { get; set; }
        public int CellSizeMeters { get; set; }
    }
}
