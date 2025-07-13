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

            var options = new JsonSerializerOptions();
            options.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());

            var spots = JsonSerializer.Deserialize<List<Spot>>(json, options);

            if (spots != null)
                SpotStore.SetSpots(spots);

            return SpotStore.Spots;
        }

        public static async Task<bool> SendSpotsForSyncAsync()
        {
            var spots = SpotStore.Spots;

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
    }
}
