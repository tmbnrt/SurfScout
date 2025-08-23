using SurfScout.DataStores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurfScout.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows;
using NetTopologySuite.IO.Converters;
using System.IO.Packaging;
using NetTopologySuite.Geometries;
using SurfScout.Models.GeoModel;
using SurfScout.Models.DTOs;
using SurfScout.Functions;
using Esri.ArcGISRuntime.Geometry;

namespace SurfScout.Services
{
    class SpotService
    {
        public SpotService() { }

        // Get spot list [name / location] from server
        public static async Task<IReadOnlyList<Spot>> GetSpotsAsync()
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);

            var response = await client.GetAsync("api/spots/locations");

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Error while getting spot locations from server!", "Error");
            }

            var json = await response.Content.ReadAsStringAsync();
            //var spots = JsonSerializer.Deserialize<List<Spot>>(json);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());

            var spots = JsonSerializer.Deserialize<List<Spot>>(json, options);

            if (spots != null)
                SpotStore.Instance.SetSpots(spots);

            return SpotStore.Instance.Spots;
        }

        public static async Task<bool> SendSpotsForSyncAsync()
        {
            var spots = SpotStore.Instance.Spots;

            var options = new JsonSerializerOptions
            {
                Converters = { new NetTopologySuite.IO.Converters.GeoJsonConverterFactory() },
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(spots, options);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);

            var response = await client.PostAsync("api/spots/sync", content);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                MessageBox.Show("Push to server succeeded: " + responseJson, "Server");
                return true;
            }
            else
            {
                MessageBox.Show("Error while pushing spots to the server!", "Error");
                return false;
            }
        }

        public static async Task<bool> UpdateSpotNameAsync(int spotId, string newSpotName)
        {
            if (spotId < 1 || string.IsNullOrWhiteSpace(newSpotName))
                return false;

            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);

            // Serialize new name as JSON
            var content = new StringContent(JsonSerializer.Serialize(newSpotName), Encoding.UTF8, "application/json");

            // PUT request
            var response = await client.PostAsync($"api/spots/{spotId}/rename", content);

            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> UpdateWindFetchArea(int spotId, WindFetchPolygon polygon)
        {
            if (spotId < 1 || !polygon.isClosed)
                return false;

            // ...

            // Create GeoJson
            var coords = polygon.mapPoints
                .Select(p => new List<double> { p.X, p.Y })
                .ToList();

            GeoJsonDto geoJson = new GeoJsonDto
            {
                Type = "Polygon",
                Coordinates = new List<List<List<double>>> { coords }
            };

            WindFetchAreaDto windfetch = new WindFetchAreaDto
            {
                Id = spotId,
                Geometry = geoJson
            };

            string json = JsonSerializer.Serialize(windfetch);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);

            // PUT request
            var response = await client.PutAsync($"api/spots/{spotId}/definewindfetch", content);

            return response.IsSuccessStatusCode;
        }

        public static async Task GetWindFetchArea(int spotId)
        {
            if (spotId < 1)
                return;

            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);

            var response = await client.GetAsync($"api/spots/returnwindfetch?spotId={spotId}");

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Error while getting polygon from server!", "Error");
                return;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<GeoJsonDto>(json, options);

            NetTopologySuite.Geometries.Polygon windfetchfield = Json_Helpers.CreatePolygonFromDto(dto);

            SpotStore.Instance.SetWindFetchField(spotId, windfetchfield);
        }
    }
}
