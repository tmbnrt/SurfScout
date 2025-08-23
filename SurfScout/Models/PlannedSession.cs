using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurfScout.Models;
using SurfScout.Models.DTOs;

namespace SurfScout.Models
{
    public class PlannedSession
    {
        public DateOnly Date { get; set; }
        public int SpotId { get; set; }
        public string SpotName { get; set; }
        public string SportMode { get; set; }
        public List<SessionParticipant> Participants { get; set; } = new List<SessionParticipant>();
    }
}
