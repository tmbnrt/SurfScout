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
using SurfScout.Functions.GeoFunctions;
using SurfScout.DataStores;

namespace SurfScout.Functions.GraphicsFunctions
{
    public static class OverlayFunctions
    {
        public static GraphicsOverlay CreateWindDirectionMarkers(List<WindField> windfieldmeasures, int hour)
        {
            GraphicsOverlay overlay_markers = new GraphicsOverlay();

            // Get wind field for selected time
            WindField currentWindField = windfieldmeasures.FirstOrDefault(wf => wf.Timestamp.Hour == hour);
            if (currentWindField == null)
                return null;

            foreach (WindFieldPoint wfp in currentWindField.Points)
            {
                // Convert from Nettopology in esri point
                var location = new Esri.ArcGISRuntime.Geometry.MapPoint(wfp.Location.X, wfp.Location.Y, SpatialReferences.Wgs84);

                var arrowSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, System.Drawing.Color.Blue, wfp.WindSpeedKnots);
                double size = Math.Clamp(wfp.WindSpeedKnots, 5, 50);
                arrowSymbol.Size = size / 2;

                // Add 180 degree to the angle but result is in range 0-360
                double angle = wfp.WindDirectionDegree + 180;
                if (angle >= 360)
                    angle -= 360;

                arrowSymbol.Angle = angle;

                var graphic = new Graphic(location, arrowSymbol);

                overlay_markers.Graphics.Add(graphic);
            }

            return overlay_markers;
        }

        public static Dictionary<int, GraphicsOverlay> CreateInterpolatedWindFieldOverlays(List<WindFieldInterpolated> windfieldinterpolated)
        {
            Dictionary<int, GraphicsOverlay> overlays_interpolated_by_hours = new Dictionary<int, GraphicsOverlay>();

            foreach (WindFieldInterpolated wfi in windfieldinterpolated)
            {
                GraphicsOverlay graphicsOverlay = new GraphicsOverlay();

                foreach (var cell in wfi.Cells)
                {
                    // Create ArcGIS polygon based on MapPoint list
                    var pointCollection = new PointCollection(SpatialReferences.Wgs84);
                    foreach (var pt in cell.PolygonPoints)
                        pointCollection.Add(pt);

                    var arcgisPolygon = new Polygon(pointCollection);

                    var windspeedColor = cell.IntensityColor ?? ColorDefinition.GetColorForWindspeed(cell.SpeedKnots);

                    var fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, windspeedColor, null);
                    var cellGraphic = new Graphic(arcgisPolygon, fillSymbol);

                    graphicsOverlay.Graphics.Add(cellGraphic);
                }

                if (graphicsOverlay != null)
                    overlays_interpolated_by_hours.Add(wfi.Timestamp.Hour, graphicsOverlay);
            }

            return overlays_interpolated_by_hours;
        }

        public static GraphicsOverlay CreateGraphicsOverlayWindmarkers_Old(WindField windfield, NTS.Polygon ntsPolygon)
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
            //List<MapPoint> rasterPoints = GenerateRasterPointsInPolygonESRI(ntsPolygon, cellSizeMeters);
            //foreach (MapPoint point in rasterPoints)
            //{
            //    double interpolatedWindspeed = InterpolateIDW(point, windPoints, windspeedValues);
            //    var windspeedColor = GetColorForWindspeed(interpolatedWindspeed);
            //
            //
            //    //var cell = CreateCellPolygon(point, 1000);
            //    //var cell = GeometryEngine.Buffer(point, 0.01);
            //    var envelope = new Envelope(
            //        point.X - 0.005, point.Y - 0.005,
            //        point.X + 0.005, point.Y + 0.005,
            //        SpatialReferences.Wgs84);
            //    var cell = envelope;
            //
            //    SimpleFillSymbol symbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, windspeedColor, null);
            //    var cellGraphic = new Graphic(cell, symbol);
            //    graphicsOverlay.Graphics.Add(cellGraphic);
            //}

            return graphicsOverlay;
        }
    }
}
