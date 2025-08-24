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
using System.Net.Http.Json;
using System.Net;

namespace SurfScout.Services
{
    class SessionPlannerService
    {
        public SessionPlannerService() { }

        public static async Task GetOwnPlannedSessions()
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);
            
            var response = await client.GetAsync($"api/plannedsessions/getusersessions?userId={UserSession.UserId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    PlannedSessionStore.Instance.PlannedSessionsOwn.Clear();
                    return;
                }

                MessageBox.Show("Error while getting planned sessions from server!", "Error");
                return;
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var plannedSessions = JsonSerializer.Deserialize<List<PlannedSession>>(json, options);

            if (plannedSessions != null)
                foreach (var ps in plannedSessions)
                {
                    PlannedSessionStore.Instance.AddPlannedSessionsOwn_AllModes(ps);
                    if (ps.SportMode == UserSession.SelectedSportMode)
                        PlannedSessionStore.Instance.AddPlannedSessionOwn(ps);
                }
        }

        public static async Task GetForeignPlannedSessions()
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);
            
            var response = await client.GetAsync($"api/plannedsessions/sessionsofuser?userId={UserSession.UserId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    PlannedSessionStore.Instance.PlannedSessionsOwn.Clear();
                    return;
                }

                MessageBox.Show("Error while getting planned sessions from server!", "Error");
                return;
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var plannedSessions = JsonSerializer.Deserialize<List<PlannedSession>>(json, options);

            if (plannedSessions != null)
                foreach (var ps in plannedSessions)
                    if (ps.SportMode == UserSession.SelectedSportMode)
                        PlannedSessionStore.Instance.AddPlannedSessionForeign(ps);
        }

        public static async Task<PlannedSession> ParticipateAtSession(int sessionId, TimeOnly start, TimeOnly end)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);

            var url = $"api/plannedsessions/participate?sessionId={sessionId}&userId={UserSession.UserId}&startTime={start:HH\\:mm}&endTime={end:HH\\:mm}";

            var response = await client.PutAsync(url, null);

            // If NotFound popup message box to tell user to create a new session
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                MessageBox.Show("Session not found. Please create a new session.", "Session Not Found");
                PlannedSessionStore.Instance.RemovePlannedForeignSession(sessionId);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Error while participating at session!", "Error");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var plannedSession = JsonSerializer.Deserialize<PlannedSession>(json, options);

            if (plannedSession != null)
                PlannedSessionStore.Instance.AddPlannedSessionOwn(plannedSession);

            return plannedSession;
        }

        // Add a session plan
        public static async Task<PlannedSession> PostNewSessionPlan(PlannedSession newPlannedSession)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);

            var jsonContent = JsonSerializer.Serialize(newPlannedSession);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"api/plannedsessions/addsession?userId={UserSession.UserId}",
                                                  content);

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Error while creating planned session!", "Error");
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var plannedSession = JsonSerializer.Deserialize<PlannedSession>(jsonResponse, options);

            return plannedSession;
        }




        // TODO: Send request to API to delete user (by id) from the planned session
        // in backend: delete session if no participants left

    }
}
