using SurfScout.DataStores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using SurfScout.Models;
using SurfScout.Models.DTOs;
using SurfScout.Models.WindModel;
using SurfScout.Utilities.GeoJson;
using SurfScout.Functions.GraphicsFunctions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows;
using NetTopologySuite.IO.Converters;
using System.Net.Http.Json;
using System.Net;
using System.IO;
using System.Drawing;
using Esri.ArcGISRuntime.Geometry;

namespace SurfScout.Services
{
    class WindFieldService
    {
        public WindFieldService() { }

        // Request measure wind field data for a session
        public static async Task<IReadOnlyList<WindField>> GetWindFieldMeasureDataAsync(Session session)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);

            var response = await client.GetAsync($"api/windfields/sessionwindfield?sessionId={session.Id}");

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Error while getting wind field data from server!", "API-Error");
                return null!;
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new GeoJsonConverterFactory());

            var windFields = JsonSerializer.Deserialize<List<WindField>>(json, options);
            if (windFields != null)
                SessionStore.Instance.PutWindFieldData(session.Id, windFields);

            return windFields;
        }

        // Request interpolated wind field data for a session
        public static async Task<IReadOnlyList<WindFieldInterpolated>> GetInterpolatedWindFieldDataAsync(Session session)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);

            var response = await client.GetAsync($"api/windfields/interpolatedwindfields?sessionId={session.Id}");

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Error while getting interpolated wind field data from server!", "API-Error");
                return null!;
            }

            // ...
            using var zipStream = await response.Content.ReadAsStreamAsync();
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

            var windFields = new List<WindFieldInterpolated>();

            foreach (var zip in archive.Entries)
            {
                if (!zip.Name.EndsWith(".geojson.gz", StringComparison.OrdinalIgnoreCase))
                    continue;

                using var entryStream = zip.Open();
                using var gzipStream = new GZipStream(entryStream, CompressionMode.Decompress);
                using var reader = new StreamReader(gzipStream, Encoding.UTF8);

                string geoJson = await reader.ReadToEndAsync();

                try
                {
                    var featureCollection = JsonSerializer.Deserialize<GeoJsonFeatureCollection>(geoJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var windField = new WindFieldInterpolated
                    {
                        Date = DateOnly.Parse(featureCollection.Metadata.Date),
                        Timestamp = TimeOnly.Parse(featureCollection.Metadata.Timestamp),
                        CellSizeMeters = featureCollection.Metadata.CellSizeMeters,
                        Cells = new List<WindFieldCellInterpolated>()
                    };

                    foreach (var feature in featureCollection.Features)
                    {
                        var geometry = (JsonElement)feature.Geometry;
                        var coords = geometry.GetProperty("coordinates");

                        var polygonPoints = new List<MapPoint>();

                        foreach (var point in coords[0].EnumerateArray())
                        {
                            double lon = point[0].GetDouble();
                            double lat = point[1].GetDouble();
                            polygonPoints.Add(new MapPoint(lon, lat, SpatialReferences.Wgs84));
                        }

                        var windSpeed = Convert.ToDouble(feature.Properties["windSpeedKnots"]);

                        var cell = new WindFieldCellInterpolated
                        {
                            SpeedKnots = windSpeed,
                            PolygonPoints = polygonPoints,
                            IntensityColor = ColorDefinition.GetColorForWindspeed(windSpeed)
                        };

                        windField.Cells.Add(cell);
                    }

                    windFields.Add(windField);
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Error while parsing GeoJSON: {ex.Message}", "Parsing Error");
                }
            }

            return windFields;
        }
    }
}
