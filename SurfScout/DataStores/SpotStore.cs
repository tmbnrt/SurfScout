using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
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

        public static void ClearSpots()
        {
            _spots.Clear();
        }

        public static int GetLatestId()
        {
            int latestId = 0;

            foreach (var spot in _spots)
                if (spot.Id > latestId)
                    latestId = spot.Id;

            return latestId;
        }

        public static void RenameSpot(int id, string newName)
        {
            foreach (Spot spot in _spots)
                if (spot.Id == id)
                    spot.Name = newName;
        }

        public static void SetWindFetchField(int id, Polygon polygon)
        {
            foreach (Spot spot in _spots)
                if (spot.Id == id)
                    spot.WindFetchPolygon = polygon;
        }
    }
}