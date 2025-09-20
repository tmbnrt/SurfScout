using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using NetTopologySuite;
using NTS = NetTopologySuite.Geometries;
using SurfScout.Models.WindModel;
using SurfScout.Functions.GraphicsFunctions;

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

        public static List<NTS.Point> GenerateRasterPointsInPolygon(NTS.Polygon ntsPolygon,
                                                                    double spacingMeters)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var pointsInsidePolygon = new List<NTS.Point>();

            // Polygon bounds
            var env = ntsPolygon.EnvelopeInternal;
            double minLon = env.MinX;
            double maxLon = env.MaxX;
            double minLat = env.MinY;
            double maxLat = env.MaxY;

            // Approximate geodetic step size using lat-dependent conversion (1 degree lat ~ 111320m)
            double stepLat = spacingMeters / 111320.0;

            // Use center latitude for approximation (1 degree lng ~ 111320 * cos(lat) m)
            double centerLatRad = (minLat + maxLat) / 2.0 * Math.PI / 180.0;
            double metersPerDegreeLon = 111320.0 * Math.Cos(centerLatRad);
            double stepLon = spacingMeters / metersPerDegreeLon;

            // Create raster points
            for (double lon = minLon; lon <= maxLon; lon += stepLon)
            {
                for (double lat = minLat; lat <= maxLat; lat += stepLat)
                {
                    var candidate = geometryFactory.CreatePoint(new NTS.Coordinate(lon, lat));

                    if (ntsPolygon.Contains(candidate))
                        pointsInsidePolygon.Add(candidate);
                }
            }

            return pointsInsidePolygon;
        }

        public static List<MapPoint> GenerateRasterPointsInPolygonESRI(NTS.Polygon ntsPolygon,
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
                    var coor = new NTS.Coordinate(candidate.X, candidate.Y);
                    var ntsPoint = new NTS.Point(coor);

                    if (ntsPolygon.Contains(ntsPoint))
                        pointsInsidePolygon.Add(candidate);
                }
            }

            return pointsInsidePolygon;
        }        

        public static GraphicsOverlay CreateGraphicsOverlayInterpolationIDW(WindField windfield,
                                                                            NTS.Polygon ntsPolygon,
                                                                            int cellSizeMeters)
        {
            GraphicsOverlay graphicsOverlay = new GraphicsOverlay();

            // Convert to ArcGIS Polygon
            var pointCollection = new PointCollection(SpatialReferences.Wgs84);
            foreach (var coord in ntsPolygon.Coordinates)
                pointCollection.Add(coord.X, coord.Y);

            var arcgisPolygon = new Esri.ArcGISRuntime.Geometry.Polygon(pointCollection);

            // Create MapPoint list and windspeed list
            List<MapPoint> windPoints = new List<MapPoint>();
            List<double> windspeedValues = new List<double>();
            foreach (WindFieldPoint wfp in windfield.Points)
            {
                windPoints.Add(new MapPoint(wfp.Location.X, wfp.Location.Y, SpatialReferences.Wgs84));
                windspeedValues.Add(wfp.WindSpeedKnots);
            }

            // Divide polygon into raster points and interpolate values to create colored cells
            List<MapPoint> rasterPoints = GenerateRasterPointsInPolygonESRI(ntsPolygon, cellSizeMeters);
            foreach (MapPoint point in rasterPoints)
            {
                double interpolatedWindspeed = InterpolateIDW(point, windPoints, windspeedValues);
                var windspeedColor = ColorDefinition.GetColorForWindspeed(interpolatedWindspeed);


                //var cell = CreateCellPolygon(point, 1000);
                //var cell = GeometryEngine.Buffer(point, 0.01);
                var envelope = new Envelope(
                    point.X - 0.005, point.Y - 0.005,
                    point.X + 0.005, point.Y + 0.005,
                    SpatialReferences.Wgs84);
                var cell = envelope;

                SimpleFillSymbol symbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, windspeedColor, null);
                var cellGraphic = new Graphic(cell, symbol);
                graphicsOverlay.Graphics.Add(cellGraphic);
            }

            return graphicsOverlay;
        }

        private static Esri.ArcGISRuntime.Geometry.Polygon CreateCellPolygon(MapPoint center, double sizeMeters)
        {
            var halfSize = sizeMeters / 2;

            var offsets = new[]
            {
                new MapPoint(center.X - halfSize, center.Y - halfSize),
                new MapPoint(center.X + halfSize, center.Y - halfSize),
                new MapPoint(center.X + halfSize, center.Y + halfSize),
                new MapPoint(center.X - halfSize, center.Y + halfSize)
            };

            return new Esri.ArcGISRuntime.Geometry.Polygon(new PointCollection(offsets));
        }
        
        private static double InterpolateIDW(MapPoint target, List<MapPoint> sources, List<double> values)
        {
            double numerator = 0;
            double denominator = 0;
            for (int i = 0; i < sources.Count; i++)
            {
                double dist = GeometryEngine.Distance(target, sources[i]);
                // Set IDW weight
                double weight = 1.0 / Math.Pow(dist, 2);
                numerator += weight * values[i];
                denominator += weight;
            }
            return numerator / denominator;
        }
    }
}
