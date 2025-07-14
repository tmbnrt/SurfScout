using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using NetTopologySuite.Geometries;

namespace SurfScout.Models
{
    public class Spot
    {
        public int id { get; set; }
        public string name { get; set; }
        public Point location { get; set; }

        // Navigation property
        public List<Session> sessions { get; set; }

        public bool CheckWithinDistance(double longitude, double latitude, double maxDistance)
        {
            // Calculate Distance with Geometry engine (Haversine formula)
            var spotPoint = new MapPoint(location.X, location.Y, SpatialReferences.Wgs84);
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
