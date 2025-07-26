using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using NetTopologySuite.Geometries;

namespace SurfScout.Functions.GeoFunctions
{
    public static class SpatialOperations
    {
        public static bool IsWithinDistance(double ref_lat, double ref_lng,
                                               double new_lat, double new_lng,
                                               double maxDistance)
        {
            // Calculate Distance with Geometry engine (Haversine formula)
            var refPoint = new MapPoint(ref_lat, ref_lng, SpatialReferences.Wgs84);
            var newPoint = new MapPoint(new_lat, new_lng, SpatialReferences.Wgs84);

            // ESRI Geometry engine to distance in [m]
            var result = GeometryEngine.DistanceGeodetic(
                refPoint,
                newPoint,
                LinearUnits.Meters,
                null,
                GeodeticCurveType.Geodesic
            );

            double distanceInMeters = result.Distance;

            return distanceInMeters <= maxDistance;
        }
    }
}
