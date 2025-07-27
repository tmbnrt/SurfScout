using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml;
using NetTopologySuite.Geometries;
using Esri.ArcGISRuntime.Geometry;
using SurfScout.Functions.GeoFunctions;

namespace SurfScout.Models.GeoModel
{
    public class WindFetchPolygon
    {
        public NetTopologySuite.Geometries.Polygon polygon {  get; private set; }
        private List<double[]> coordinates;     // Global coordinates
        public List<MapPoint> mapPoints;
        //private List<double[]> mapPosition;     // Map point for visualization
        public bool isClosed;
        

        public WindFetchPolygon()
        {
            coordinates = new List<double[]>();
            mapPoints = new List<MapPoint>();
            //this.mapPosition = new List<double[]>();
            isClosed = false;
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

            coordinates.Add(new double[] { lat, lng });
            var mapPoint = new MapPoint(lng, lat, SpatialReferences.Wgs84);
            mapPoints.Add(mapPoint);

            if (coordinates.Count > 2 && coordinates.First()[0] == lat && coordinates.First()[1] == lng)
            {
                isClosed = true;
                CreatePolygon();
            }

            return coordinates;
        }

        private void CreatePolygon()
        {
            var coords = mapPoints.Select(p => new Coordinate(p.X, p.Y)).ToList();
            var shell = new LinearRing(coords.ToArray());
            polygon = new NetTopologySuite.Geometries.Polygon(shell);
        }

        public void AddExistingPolygon(NetTopologySuite.Geometries.Polygon polygon)
        {
            this.polygon = polygon;

            var shell = polygon.Shell;

            // Iteriere über alle Koordinaten
            foreach (var coor in shell.Coordinates)
            {
                var point = new MapPoint(coor.X, coor.Y, SpatialReferences.Wgs84);
                mapPoints.Add(point);
            }
        }

        public void DeleteLastPoint()
        {
            if (coordinates.Count > 0)
                coordinates.RemoveAt(coordinates.Count - 1);

            if (mapPoints.Count > 0)
                mapPoints.RemoveAt(mapPoints.Count - 1);

            isClosed = false;
            polygon = null!;
        }
    }
}
