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
    class SessionService
    {
        public SessionService() { }

        // Get session list from server
        public static async Task<IReadOnlyList<Session>> GetSessionsAsync(Spot spot)
        {

            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);

            var response = await client.GetAsync($"api/sessions/spotsessions?spot={spot.name}");

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Error while getting spot locations from server!", "Error");
                return Array.Empty<Session>();
            }

            var json = await response.Content.ReadAsStringAsync();
            //var spots = JsonSerializer.Deserialize<List<Spot>>(json);

            var options = new JsonSerializerOptions();
            options.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());

            var sessions = JsonSerializer.Deserialize<List<Session>>(json, options);

            if (sessions != null)
                SessionStore.SetSessions(sessions);

            return SessionStore.Sessions;
        }
    }
}
