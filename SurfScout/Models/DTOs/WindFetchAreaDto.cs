using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace SurfScout.Models.DTOs
{
    public class WindFetchAreaDto
    {
        public int Id { get; set; }
        public GeoJsonDto Geometry { get; set; }
    }
}
