using SurfScout.Models.WindModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfScout.Models.DTOs
{
    public class SessionDto
    {
        public int Id;
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int SpotId { get; set; }
        public int UserId { get; set; }
        public string Sport { get; set; }
        public double Sail_size { get; set; }
        public int Rating { get; set; }
        public string Wave_height { get; set; }
        public double? WindSpeedKnots { get; set; }
        public double? WindDirectionDegree { get; set; }
        public ICollection<WindField>? WindFields { get; set; } = new List<WindField>();
    }
}
