using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml;
using System.Text.Json;
using NetTopologySuite.Geometries;
using Esri.ArcGISRuntime.Geometry;

namespace SurfScout.Functions.GeoFunctions
{
    public class PolygonEditor
    {
        public NetTopologySuite.Geometries.Polygon polygon {  get; private set; }
        public string json { get; private set; }
        private List<double[]> coordinates;     // Global coordinates
        public List<MapPoint> mapPoints;
        //private List<double[]> mapPosition;     // Map point for visualization
        public bool isClosed;
        

        public PolygonEditor()
        {
            this.coordinates = new List<double[]>();
            this.mapPoints = new List<MapPoint>();
            //this.mapPosition = new List<double[]>();
            this.isClosed = false;
        }

        public List<double[]> SetPoint(double[] point)
        {
            double lat = point[0];
            double lng = point[1];

            // Check first point nearby (20 km) - if ture --> close polygon
            if (coordinates.Count > 0)
            {
                if (SpatialOperations.IsWithinDistance(coordinates.First()[0], coordinates.First()[1],
                                                   lat, lng, 20000))
                {
                    lat = coordinates.First()[0];
                    lng = coordinates.First()[1];
                }
            }

            this.coordinates.Add(new double[] { lat, lng });
            var mapPoint = new MapPoint(lng, lat, SpatialReferences.Wgs84);
            this.mapPoints.Add(mapPoint);

            if (coordinates.Count > 2 && coordinates.First()[0] == lat && coordinates.First()[1] == lng)
            {
                isClosed = true;
                CreatePolygon();
                CreateGeoJson();
            }

            return coordinates;
        }

        private void CreatePolygon()
        {
            var coords = mapPoints.Select(p => new Coordinate(p.X, p.Y)).ToList();
            var shell = new LinearRing(coords.ToArray());
            this.polygon = new NetTopologySuite.Geometries.Polygon(shell);
        }

        public void DeleteLastPoint()
        {
            if (coordinates.Count > 0)
                this.coordinates.RemoveAt(coordinates.Count - 1);

            if (mapPoints.Count > 0)
                this.mapPoints.RemoveAt(mapPoints.Count - 1);

            this.isClosed = false;
            this.json = "";
            this.polygon = null!;

            //return coordinates;
        }

        private void CreateGeoJson()
        {
            var coords = mapPoints
                .Select(p => new double[] { p.X, p.Y })
                .ToList();

            var geoJson = new
            {
                type = "Polygon",
                coordinates = new List<List<double[]>> { coords }
            };

            this.json = JsonSerializer.Serialize(geoJson);
        }
    }
}
