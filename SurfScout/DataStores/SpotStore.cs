using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using SurfScout.Models;

namespace SurfScout.DataStores
{
    public class SpotStore
    {
        private static SpotStore _instance;
        public static SpotStore Instance => _instance ??= new SpotStore();

        private List<Spot> _spots = new();
        public IReadOnlyList<Spot> Spots => _spots;

        public void SetSpots(IEnumerable<Spot> spots)
        {
            _spots = new List<Spot>(spots);
        }

        public void AddSpot(Spot spot)
        {
            _spots.Add(spot);
        }

        public void ClearSpots()
        {
            _spots.Clear();
        }

        public int GetLatestId()
        {
            int latestId = 0;

            foreach (var spot in _spots)
                if (spot.Id > latestId)
                    latestId = spot.Id;

            return latestId;
        }

        public void RenameSpot(int id, string newName)
        {
            foreach (Spot spot in _spots)
                if (spot.Id == id)
                    spot.Name = newName;
        }

        public void SetWindFetchField(int id, Polygon polygon)
        {
            foreach (Spot spot in _spots)
                if (spot.Id == id)
                {
                    spot.WindFetchPolygon = polygon;
                    spot.GenerateRasterPoints();
                }                    
        }
    }
}
