using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace SurfScout.Models
{
    public class Spot
    {
        public int id { get; set; }
        public string name { get; set; }
        public Point location { get; set; }

        // Navigation property
        public List<Session> sessions { get; set; }
    }
}
