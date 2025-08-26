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

            var response = await client.GetAsync($"api/userconnections/pending?userId={UserSession.UserId}");
            
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

        public static async Task<IReadOnlyList<User>> GetMyConnections()
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);

            var response = await client.GetAsync($"api/userconnections/getconnections?userId={UserSession.UserId}");

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Error while getting connections from server!", "Error");
                return new List<User>();
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var friends = JsonSerializer.Deserialize<List<User>>(json, options);

            if (friends != null)
                return friends;

            return new List<User>();
        }

        public static async Task<bool> SendConnectionRequest(string addresseeName)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            var connectionDto = new UserConnectionDto
            {
                RequesterId = UserSession.UserId,
                RequesterUsername = UserSession.Username,
                AddresseeUsername = addresseeName
            };

            //var json = JsonSerializer.Serialize(connectionDto);
            //var content = new StringContent(json, Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);

            var response = await client.PostAsJsonAsync($"api/userconnections/newrequest", connectionDto);

            if (response.IsSuccessStatusCode)
                return true;
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                MessageBox.Show("Connection request already exists.");
                return false;
            }                
            else
            {
                MessageBox.Show("Error while sending connection request.");
                return false;
            }
        }

        public static async Task<bool> AcceptConnectionRequest(string requesterName)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            var connectionDto = new UserConnectionDto
            {
                AddresseeId = UserSession.UserId,
                AddresseeUsername = UserSession.Username,
                RequesterUsername = requesterName
            };

            //var json = JsonSerializer.Serialize(connectionDto);
            //var content = new StringContent(json, Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);

            var response = await client.PostAsJsonAsync($"api/userconnections/acceptrequest", connectionDto);

            if (response.IsSuccessStatusCode)
                return true;             

            if (response.StatusCode == HttpStatusCode.NotFound)
                MessageBox.Show("Connection request not found.");
            else
                MessageBox.Show("Error while accepting connection request.");

            return false;
        }

        public static async Task<bool> RejectConnectionRequest(string requesterName)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            var connectionDto = new UserConnectionDto
            {
                AddresseeId = UserSession.UserId,
                AddresseeUsername = UserSession.Username,
                RequesterUsername = requesterName
            };

            //var json = JsonSerializer.Serialize(connectionDto);
            //var content = new StringContent(json, Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);

            var response = await client.PostAsJsonAsync($"api/userconnections/rejectrequest", connectionDto);

            if (response.IsSuccessStatusCode)
                return true;

            if (response.StatusCode == HttpStatusCode.NotFound)
                MessageBox.Show("Connection request not found.");
            else
                MessageBox.Show("Error while rejecting connection request.");

            return false;
        }
    }
}
