using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfScout.Utilities.GeoJson
{
    public class GeoJsonFeatureCollection
    {
        public GeoJsonMetadata Metadata { get; set; }
        public List<GeoJsonFeature> Features { get; set; }
    }
}
