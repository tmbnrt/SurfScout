using SurfScout.DataStores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurfScout.Models;
using SurfScout.Models.DTOs;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows;
using NetTopologySuite.IO.Converters;
using System.Net.Http.Json;
using System.Net;

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
            
             var response = await client.GetAsync($"api/sessions/spotsessions?spotId={spot.Id}");

             if (!response.IsSuccessStatusCode)
             {
                 if (response.StatusCode == HttpStatusCode.NotFound)
                 {
                     return Array.Empty<Session>();
                 }

                 MessageBox.Show("Error while getting sessions from server!", "Error");
                 return Array.Empty<Session>();
             }

             var json = await response.Content.ReadAsStringAsync();

             var options = new JsonSerializerOptions
             {
                 PropertyNameCaseInsensitive = true
             };

             var sessions = JsonSerializer.Deserialize<List<Session>>(json, options);

             // Assign known spot instances and user to sessions
             var spotLookup = SpotStore.Spots.ToDictionary(s => s.Id);
             var userLookup = AllUserStore.Users.ToDictionary(s => s.Id);
             foreach (var session in sessions)
             {
                 if (spotLookup.TryGetValue(session.Spotid, out var knownSpot))
                     session.Spot = knownSpot;
                 if (userLookup.TryGetValue(session.UserId, out var user))
                     session.User = user;
             }

             if (sessions != null)
                 SessionStore.SetSessions(sessions);

             return SessionStore.Sessions;
        }

        // Post to server
        public static async Task PostSessionAsync(Session session)
        {
            var dto = new SessionDto
            {
                Date = session.Date,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                SpotId = session.Spot.Id,
                UserId = session.UserId,
                Sail_size = session.Sail_size,
                Rating = session.Rating,
                Wave_height = session.Wave_height
            };

            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);

            var response = await client.PostAsJsonAsync("api/sessions/savesession", dto);
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Error while storing session on server!", "API-Error");
            }
        }
    }
}
