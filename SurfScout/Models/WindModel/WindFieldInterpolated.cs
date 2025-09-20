using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfScout.Models.WindModel
{
    public class WindFieldInterpolated
    {
        public DateOnly Date { get; set; }
        public TimeOnly Timestamp { get; set; }
        public double CellSizeMeters { get; set; }
        public List<WindFieldCellInterpolated> Cells { get; set; }
    }
}
