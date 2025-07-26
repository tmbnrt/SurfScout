using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml;
using System.Text.Json;

namespace SurfScout.Functions.GeoFunctions
{
    public class PolygonEditor
    {
        public string json { get; private set; }
        private List<double[]> coordinates;     // Global coordinates
        private List<double[]> mapPosition;     // Map point for visualization
        public bool isClosed;
        

        public PolygonEditor()
        {
            this.coordinates = new List<double[]>();
            this.mapPosition = new List<double[]>();
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
            
            // Also add map point
            // ...

            if (coordinates.Count > 2 && coordinates.First()[0] == lat && coordinates.First()[1] == lng)
            {
                isClosed = true;
                CreateJson();
            }

            return coordinates;
        }

        public List<double[]> DeleteLastPoint()
        {
            if (coordinates.Count > 0)
                this.coordinates.RemoveAt(coordinates.Count - 1);

            if (mapPosition.Count > 0)
                this.mapPosition.RemoveAt(mapPosition.Count - 1);

            this.isClosed = false;
            this.json = "";

            return coordinates;
        }

        private void CreateJson()
        {
            var geoJson = new
            {
                type = "Polygon",
                coordinates = new List<List<double[]>> { coordinates }
            };

            this.json = JsonSerializer.Serialize(coordinates);
        }
    }
}
