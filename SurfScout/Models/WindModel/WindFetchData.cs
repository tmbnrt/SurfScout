using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using SurfScout.Functions.GeoFunctions;

namespace SurfScout.Models.WindModel
{
    // This class represents the windfetch data for a spot (instance can be real data or foracast data)
    public class WindFetchData
    {
        public int SpotId { get; set; }
        public List<MapPoint>? VectorPoints { get; set; }
        
        // Dictionaries containing the hourly wind data sets - Key: i.e. AROME or ICON
        public Dictionary<string, List<WindFetchDataSet>> Model_ForecastDataSet { get; set; } = new();
        public List<WindFetchDataSet> HistoricDataSet { get; set; } = new();

        public WindFetchData(int spotId, NetTopologySuite.Geometries.Polygon? polygon)
        {
            if (polygon == null)
                return;

            this.SpotId = spotId;

            // Using ESRI engine
            //this.RasterPoints = SpatialOperations.GenerateRasterPointsInPolygon(polygon, 25000);

            List<NetTopologySuite.Geometries.Point> ntsPoints = SpatialOperations.GenerateRasterPointsInPolygon(polygon, 25000);

            // Convert NTS points to ESRI MapPoints
            this.VectorPoints = ntsPoints.Select(p => new MapPoint(p.X, p.Y, SpatialReferences.Wgs84)).ToList();
        }

    }
}
