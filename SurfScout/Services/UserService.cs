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

namespace SurfScout.Services
{
    class UserService
    {
        public UserService() {}

        public async Task<LoginResponse?> LoginAsync(string username, string password)
        {
            var loginRequest = new
            {
                username = username.Trim(),
                password_hash = password.Trim()
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/api/Users/login")
            };

            try
            {
                var response = await client.PostAsync("login", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result != null && result.Success)
                    {
                        UserSession.JwtToken = result.Token;
                        UserSession.UserId = result.User.Id;
                        UserSession.Username = result.User.Username;
                        UserSession.Role = result.User.Role;
                        UserSession.SelectedSportMode = result.User.Sports[0];  // Default to first sport in list
                        return result;
                    }
                }
                else
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(errorText, "Login failed.", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}", "Network problem", MessageBoxButton.OK);
            }

            return null;
        }

        public async Task<bool> RegisterUserAsync(string username, string email, string password, string[] sportSelection)
        {
            if (!EmailCheck.EmailIsValid(email))
            {
                MessageBox.Show("Email address is not valid.");
                return false;
            }

            if (sportSelection.Length < 1)
            {
                MessageBox.Show("Please select wind sports.");
                return false;
            }

            var user = new
            {
                username = username.Trim(),
                email = email.Trim(),
                password_hash = password.Trim(),
                sports = sportSelection
            };

            var json = JsonSerializer.Serialize(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:7190/api/Users/register");

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
        
        public static async Task<IReadOnlyList<User>> GetAllUsersAsync()
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7190/")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", UserSession.JwtToken);

            var response = await client.GetAsync($"api/users/getallusers");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return Array.Empty<User>();
                }

                MessageBox.Show("Error while getting user info from server!", "Error");
                return Array.Empty<User>();
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var users = JsonSerializer.Deserialize<List<User>>(json, options);

            if (users != null)
                AllUserStore.SetAllUsers(users);

            return AllUserStore.Users;
        }
    }
}
