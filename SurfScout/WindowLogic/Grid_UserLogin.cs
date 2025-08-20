using SurfScout.DataStores;
using SurfScout.Models.DTOs;
using SurfScout.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace SurfScout.WindowLogic
{
    class Grid_UserLogin
    {
        MainWindow win;

        public Grid_UserLogin(object sender, RoutedEventArgs e, MainWindow window)
        {
            this.win = window;

            win.LoginToggle.Click += LoginToggle_Click;
            win.RegisterToggle.Click += RegisterToggle_Click;

            win.buttonLogin.Click += ButtonLogin_Click;
            win.buttonRegister.Click += ButtonRegister_Click;
            win.buttonLogout.Click += ButtonLogout_Click;
            win.buttonSwitchSportMode.Click += ButtonSwitchSportMode_Click;
        }

        private void LoginToggle_Click(object sender, RoutedEventArgs e)
        {
            win.EmailLabel.Visibility = Visibility.Collapsed;
            win.EmailBox.Visibility = Visibility.Collapsed;
            win.SportLabel.Visibility = Visibility.Collapsed;
            win.SportSelectionPanel.Visibility = Visibility.Collapsed;

            win.buttonLogin.Visibility = Visibility.Visible;
            win.buttonRegister.Visibility = Visibility.Collapsed;

            win.LoginToggle.IsChecked = true;
            win.RegisterToggle.IsChecked = false;
        }

        private void RegisterToggle_Click(object sender, RoutedEventArgs e)
        {
            win.EmailLabel.Visibility = Visibility.Visible;
            win.EmailBox.Visibility = Visibility.Visible;
            win.SportLabel.Visibility = Visibility.Visible;
            win.SportSelectionPanel.Visibility = Visibility.Visible;

            win.buttonLogin.Visibility = Visibility.Collapsed;
            win.buttonRegister.Visibility = Visibility.Visible;

            win.RegisterToggle.IsChecked = true;
            win.LoginToggle.IsChecked = false;
        }

        private void ButtonSwitchSportMode_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(win.comboSportOptions.Text))
                return;

            // Set the selected sport mode in the user session
            UserSession.SelectedSportMode = win.comboSportOptions.Text;
        }

        private async void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            // Reset all user data and clear session and spot logs
            UserSession.Reset();
            SpotStore.ClearSpots();
            SessionStore.ClearSessions();

            // If succeeded -> swap grid to *login
            win.ChangeGrid(win.Dashboard);
        }

        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = win.UsernameBox.Text.Trim();
            string password = win.PasswordBox.Password;

            // API login
            var service = new UserService();
            var response = await service.LoginAsync(username, password);

            try
            {
                if (response != null && response.Success)
                {
                    UserSession.JwtToken = response.Token;
                    UserSession.Username = response.User.Username;
                    UserSession.UserId = response.User.Id;
                    if (!string.IsNullOrWhiteSpace(response.User.Role))
                        UserSession.Role = response.User.Role;

                    MessageBox.Show($"Login successful. Welcome back, {username}!");
                    win.UsernameBox.Text = string.Empty;
                    win.PasswordBox.Password = string.Empty;

                    // Call and store connection requesters
                    var userConnections = await UserConnectionService.GetConnectionRequesters();

                    List<string> requesters = new List<string>();
                    foreach (var uc in userConnections)
                        requesters.Add(uc.RequesterUsername);
                    UserSession.AddConnectionRequesters(requesters);

                    // Store user info
                    //UserStore.Role

                    // Change button name to user name and swap grid from *login to *userinfo
                    //win.LoggedUser.Text = response.User.Username;
                    win.textBlockSportMode.Text = UserSession.SelectedSportMode;

                    // Fill combo box with sport modes
                    win.comboSportOptions.Items.Clear();
                    foreach (var sport in response.User.Sports)
                        win.comboSportOptions.Items.Add(sport);

                    win.ChangeGrid(win.Dashboard);

                    // Get User data from server
                    await UserService.GetAllUsersAsync();
                }
                else
                {
                    return;
                }
            }
            catch
            {
                return;
            }            
        }

        private async void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = win.UsernameBox.Text.Trim();
            string password = win.PasswordBox.Password;
            string email = win.EmailBox.Text.Trim();

            // Add sport selection
            List<string> sportlist = new List<string>();
            if (win.ToggleWindsurfing.IsChecked == true)
                sportlist.Add("Windsurfing");
            if (win.ToggleKitesurfing.IsChecked == true)
                sportlist.Add("Kitesurfing");
            if (win.ToggleWingfoiling.IsChecked == true)
                sportlist.Add("Wingfoiling");

            string[] sports = sportlist.ToArray();

            // API Registration
            var service = new UserService();
            if (await service.RegisterUserAsync(username, email, password, sports))
            {
                MessageBox.Show("Registration successful. Please log in.");
                win.UsernameBox.Text = null;
                win.PasswordBox.Password = null;
                win.EmailBox.Text = null;
            }
        }
    }
}
