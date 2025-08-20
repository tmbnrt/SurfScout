using SurfScout.DataStores;
using SurfScout.Models;
using SurfScout.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using SurfScout.Functions.UserDataFunction;
using SurfScout.Models.DTOs;

namespace SurfScout.Services
{
    class UserConnectionService
    {
        public UserConnectionService() {}

        public static async Task<IReadOnlyList<UserConnectionDto>> GetConnectionRequesters()
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);

            var response = await client.GetAsync($"api/userconnections/pending");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return Array.Empty<UserConnectionDto>();

                MessageBox.Show("Error while getting connection requesters from server!", "Error");
                return Array.Empty<UserConnectionDto>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var userConnections = JsonSerializer.Deserialize<List<UserConnectionDto>>(json, options);

            return userConnections ?? new List<UserConnectionDto>();
        }
    }
}
