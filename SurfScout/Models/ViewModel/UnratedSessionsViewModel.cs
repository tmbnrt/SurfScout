using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfScout.Models.ViewModel
{
    public class UnratedSessionsViewModel
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public string SpotName { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string SportMode { get; set; }
    }
}
