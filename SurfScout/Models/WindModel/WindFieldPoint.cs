using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace SurfScout.Models.WindModel
{
    public class WindFieldPoint
    {
        public int Id { get; set; }
        public int WindFieldId { get; set; }
        public double WindSpeedKnots { get; set; }
        public double WindDirectionDegree { get; set; }
        public Point Location { get; set; }
    }
}
