using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;

namespace SurfScout.Models
{
    public class Session
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        //public   set: start time and end time
        public double Wave_height { get; set; }
        public int Rating { get; set; }
        public double Sail_size { get; set; }
        public int Board_volume { get; set; }
        public Spot Spot { get; set; }              // Navigation property
        public string Tide { get; set; }
        public Point Location { get; set; }          // in GeoJSON format (geo point)        

        public int UserId { get; set; }             // External key
        public User User { get; set; } = null!;     // Navigation property
    }
}
