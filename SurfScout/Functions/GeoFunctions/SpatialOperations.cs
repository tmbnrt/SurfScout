using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using NetTopologySuite;
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

        public static List<Point> GenerateRasterPointsInPolygon(NetTopologySuite.Geometries.Polygon ntsPolygon,
                                                                double spacingMeters)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var pointsInsidePolygon = new List<Point>();

            // Bounding box of polygon
            var env = ntsPolygon.EnvelopeInternal;
            double minLon = env.MinX;
            double maxLon = env.MaxX;
            double minLat = env.MinY;
            double maxLat = env.MaxY;

            // Approximate geodetic step size using latitude-dependent conversion
            // 1 degree latitude is approx 111320 meters
            double stepLat = spacingMeters / 111320.0;

            // 1 degree longitude is approx 111320 * cos(latitude) meters
            // Use center latitude for approximation
            double centerLatRad = (minLat + maxLat) / 2.0 * Math.PI / 180.0;
            double metersPerDegreeLon = 111320.0 * Math.Cos(centerLatRad);
            double stepLon = spacingMeters / metersPerDegreeLon;

            // Create raster points
            for (double lon = minLon; lon <= maxLon; lon += stepLon)
            {
                for (double lat = minLat; lat <= maxLat; lat += stepLat)
                {
                    var candidate = geometryFactory.CreatePoint(new Coordinate(lon, lat));

                    if (ntsPolygon.Contains(candidate))
                        pointsInsidePolygon.Add(candidate);
                }
            }

            return pointsInsidePolygon;
        }

        public static List<MapPoint> GenerateRasterPointsInPolygonESRI(NetTopologySuite.Geometries.Polygon ntsPolygon,
                                                                  double spacingMeters)
        {
            var pointsInsidePolygon = new List<MapPoint>();

            // Bounding box of polgon
            var env = ntsPolygon.EnvelopeInternal;
            double minLon = env.MinX;
            double maxLon = env.MaxX;
            double minLat = env.MinY;
            double maxLat = env.MaxY;

            // Move points from corner origin --> Calc geodetic step size
            MapPoint origin = new MapPoint(minLon, minLat, SpatialReferences.Wgs84);

            var movedPointsX = GeometryEngine.MoveGeodetic(
                new List<MapPoint> { origin },
                spacingMeters,
                LinearUnits.Meters,
                90,
                AngularUnits.Degrees,
                GeodeticCurveType.Geodesic
            );
            MapPoint movedX = movedPointsX.First();

            var movedPointsY = GeometryEngine.MoveGeodetic(
                new List<MapPoint> { origin },
                spacingMeters,
                LinearUnits.Meters,
                0,
                AngularUnits.Degrees,
                GeodeticCurveType.Geodesic
            );
            MapPoint movedY = movedPointsY.First();

            double stepLon = movedX.X - origin.X;
            double stepLat = movedY.Y - origin.Y;

            // Create raster points
            for (double lon = minLon; lon <= maxLon; lon += stepLon)
            {
                for (double lat = minLat; lat <= maxLat; lat += stepLat)
                {
                    var candidate = new MapPoint(lon, lat, SpatialReferences.Wgs84);

                    // Check inside polygon
                    var coor = new Coordinate(candidate.X, candidate.Y);
                    var ntsPoint = new NetTopologySuite.Geometries.Point(coor);

                    if (ntsPolygon.Contains(ntsPoint))
                        pointsInsidePolygon.Add(candidate);
                }
            }

            return pointsInsidePolygon;
        }
    }
}
