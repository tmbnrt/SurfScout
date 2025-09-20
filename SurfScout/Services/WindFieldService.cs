using SurfScout.DataStores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurfScout.Models;
using SurfScout.Models.DTOs;
using SurfScout.Models.WindModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows;
using NetTopologySuite.IO.Converters;
using System.Net.Http.Json;
using System.Net;

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

            var response = await client.GetAsync($"api/sessions/windfields?sessionId={session.Id}");

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
    }
}
