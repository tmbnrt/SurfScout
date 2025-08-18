using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using NetTopologySuite.Geometries;
using SurfScout.Models.WindModel;

namespace SurfScout.Models
{
    public class Spot
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Point Location { get; set; }
        public NetTopologySuite.Geometries.Polygon? WindFetchPolygon { get; set;}
        public List<Session>? Sessions { get; set; }
        public WindFetchForecastData? WindFetchData { get; set; }

        public void GenerateRasterPoints()
        {
            this.WindFetchData = new WindFetchForecastData(Id, WindFetchPolygon);
        }

        public bool CheckWithinDistance(double longitude, double latitude, double maxDistance)
        {
            // Calculate Distance with Geometry engine (Haversine formula)
            var spotPoint = new MapPoint(Location.X, Location.Y, SpatialReferences.Wgs84);
            var inputPoint = new MapPoint(longitude, latitude, SpatialReferences.Wgs84);

            // ESRI Geometry engine to distance in [m]
            var result = GeometryEngine.DistanceGeodetic(
                spotPoint,
                inputPoint,
                LinearUnits.Meters,
                null,
                GeodeticCurveType.Geodesic
            );

            double distanceInMeters = result.Distance;

            return distanceInMeters <= maxDistance;
        }
    }
}
