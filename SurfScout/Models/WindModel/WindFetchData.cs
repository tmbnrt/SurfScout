using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using SurfScout.Functions.GeoFunctions;

namespace SurfScout.Models.WindModel
{
    // Class representing the windfetch data for the spot
    public class WindFetchData
    {
        public int SpotId { get; set; } // Verknüpfung zum Spot
        //public Spot Spot { get; set; } // Optional: Navigation für EF Core
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<MapPoint>? RasterPoints { get; set; }
        public List<WindFetchDataSet> DataSet { get; set; } = new();

        public WindFetchData(int spotId, NetTopologySuite.Geometries.Polygon? polygon)
        {
            if (polygon == null)
                return;

            this.SpotId = spotId;
            this.RasterPoints = SpatialOperations.GenerateRasterPointsInPolygon(polygon, 10000);
        }

    }
}
