using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurfScout.Models;

namespace SurfScout.DataStores
{
    public static class SpotStore
    {
        private static List<Spot> _spots = new();
        public static IReadOnlyList<Spot> Spots => _spots;

        public static void SetSpots(IEnumerable<Spot> spots)
        {
            _spots = new List<Spot>(spots);
        }

        public static void AddSpot(Spot spot)
        {
            _spots.Add(spot);
        }
    }
}
