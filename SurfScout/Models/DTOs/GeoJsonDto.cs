using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfScout.Models.DTOs
{
    public class GeoJsonDto
    {
        public string Type { get; set; } = "Polygon";
        public List<List<double[]>> Coordinates { get; set; }
    }
}
