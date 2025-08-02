using Xunit;
using Esri.ArcGISRuntime.Geometry;
using NetTopologySuite.Geometries;
using SurfScout.Models;
using System.Drawing;

namespace SurfScout.Tests
{
    public class SpotFunctionTests
    {
        [Fact]
        public void CheckWithinDistance_Test()
        {
            // Location for Wijk aan Zee
            var spot = new Spot
            {
                Location = new NetTopologySuite.Geometries.Point(4.5941, 52.4936)
            };

            // Check distance within 30k meters
            double maxDistance = 30000;

            // Location for Ijmuiden (Test 1 --> should be true)
            double ijmuidenLng = 4.6105;
            double ijmuidenLat = 52.4603;            

            bool resultIjmuiden = spot.CheckWithinDistance(ijmuidenLng, ijmuidenLat, maxDistance);
            Assert.True(resultIjmuiden);

            // Location for Ijmuiden (Test 1 --> should be false)
            //double hanstholmLng = 8.6177;
            double hanstholmLng = 4.6105;
            //double hanstholmLat = 57.1129;
            double hanstholmLat = 52.4603;

            bool resultHanstholm = spot.CheckWithinDistance(hanstholmLng, hanstholmLat, maxDistance);
            Assert.False(resultHanstholm);
        }
    }
}
