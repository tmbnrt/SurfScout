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
                var windspeedColor = GetColorForWindspeed(interpolatedWindspeed);


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

        private static Color GetColorForWindspeed(double windSpeed)
        {
            if (windSpeed < 6)
                return Color.FromArgb(255, 255, 255);
            if (windSpeed < 7)
                return Color.FromArgb(221, 253, 252);
            if (windSpeed < 8)
                return Color.FromArgb(180, 251, 248);
            if (windSpeed < 9)
                return Color.FromArgb(126, 248, 243);
            if (windSpeed < 10)
                return Color.FromArgb(108, 247, 241);
            if (windSpeed < 11)
                return Color.FromArgb(86, 248, 201);
            if (windSpeed < 12)
                return Color.FromArgb(66, 250, 154);
            if (windSpeed < 13)
                return Color.FromArgb(31, 253, 73);
            if (windSpeed < 14)
                return Color.FromArgb(20, 253, 47);
            if (windSpeed < 15)
                return Color.FromArgb(25, 254, 0);
            if (windSpeed < 16)
                return Color.FromArgb(83, 250, 0);
            if (windSpeed < 17)
                return Color.FromArgb(132, 247, 0);
            if (windSpeed < 18)
                return Color.FromArgb(157, 246, 0);
            if (windSpeed < 19)
                return Color.FromArgb(233, 241, 0);
            if (windSpeed < 20)
                return Color.FromArgb(255, 239, 0);
            if (windSpeed < 21)
                return Color.FromArgb(255, 197, 10);
            if (windSpeed < 22)
                return Color.FromArgb(255, 171, 16);
            if (windSpeed < 23)
                return Color.FromArgb(255, 126, 26);
            if (windSpeed < 24)
                return Color.FromArgb(255, 119, 28);
            if (windSpeed < 25)
                return Color.FromArgb(255, 85, 36);
            if (windSpeed < 26)
                return Color.FromArgb(255, 48, 53);
            if (windSpeed < 27)
                return Color.FromArgb(255, 43, 73);
            if (windSpeed < 28)
                return Color.FromArgb(255, 38, 93);
            if (windSpeed < 29)
                return Color.FromArgb(255, 30, 121);
            if (windSpeed < 30)
                return Color.FromArgb(255, 25, 140);
            if (windSpeed < 31)
                return Color.FromArgb(255, 21, 156);
            if (windSpeed < 32)
                return Color.FromArgb(255, 16, 178);
            if (windSpeed < 33)
                return Color.FromArgb(255, 10, 198);
            if (windSpeed < 34)
                return Color.FromArgb(255, 8, 211);
            if (windSpeed < 35)
                return Color.FromArgb(255, 6, 220);
            if (windSpeed < 36)
                return Color.FromArgb(255, 4, 231);
            if (windSpeed < 37)
                return Color.FromArgb(255, 3, 241);

            return Color.FromArgb(182, 102, 210);

            //if (windSpeed < 3)
            //    return Color.White;
            //else if (windSpeed < 8)
            //    return Color.Blue;
            //else if (windSpeed < 15)
            //    return Color.LightBlue;
            //else if (windSpeed < 20)
            //    return Color.Cyan;
            //else if (windSpeed < 23)
            //    return Color.Green;
            //else if (windSpeed < 26)
            //    return Color.Yellow;
            //else if (windSpeed < 32)
            //    return Color.Orange;
            //else if (windSpeed < 42)
            //    return Color.Red;
            //else
            //    return Color.Purple;
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
