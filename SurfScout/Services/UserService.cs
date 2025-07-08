using SurfScout.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace SurfScout.Services
{
    class UserService
    {
        public UserService() {}

        public async Task<(bool success, string token, string username)> LoginAsync(string username, string password)
        {
            var loginRequest = new
            {
                username = username.Trim(),
                passwordHash = password.Trim()
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:7190/");

            try
            {
                var response = await client.PostAsync("login", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseBody);
                    var token = doc.RootElement.GetProperty("token").GetString();
                    var name = doc.RootElement.GetProperty("username").GetString();
                    return (true, token, name);
                }
                else
                {
                    MessageBox.Show(responseBody, "Login failed.", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}", "Network problem", MessageBoxButton.OK);
            }

            return (false, null, null);
        }

        public async Task<bool> RegisterUserAsync(string username, string password)
        {
            var user = new
            {
                username = username.Trim(),
                passwordHash = password.Trim()
            };

            var json = JsonSerializer.Serialize(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:7190/");

            try
            {
                var response = await client.PostAsync("register", content);
                if (response.IsSuccessStatusCode)
                    return true;
                else if (response.StatusCode == HttpStatusCode.Conflict)
                    MessageBox.Show("User name does already exist.");
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed: {error}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}");
            }

            return false;
        }
    }
}
