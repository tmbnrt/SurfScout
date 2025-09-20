using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SurfScout.Utilities.GeoJson
{
    public class GeoJsonFeature
    {
        public JsonElement Geometry { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}
