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
    class Grid_Dashboard
    {
        MainWindow win;

        public Grid_Dashboard(object sender, RoutedEventArgs e, MainWindow window)
        {
            this.win = window;

            // Logging
            win.LoginToggle.Click += LoginToggle_Click;
            win.RegisterToggle.Click += RegisterToggle_Click;
            win.buttonLogin.Click += ButtonLogin_Click;
            win.buttonRegister.Click += ButtonRegister_Click;
            win.buttonLogout.Click += ButtonLogout_Click;
            win.buttonChangePassword.Click += ButtonChangePassword_Click;

            // Sport modes
            win.buttonSwitchSportMode.Click += ButtonSwitchSportMode_Click;
            win.buttonAddSport.Click += ButtonAddSport_Click;

            // Connection requests
            win.buttonSendConnectionRequest.Click += ButtonSendConnectionRequest_Click;
            win.buttonAcceptConnectionRequest.Click += ButtonAcceptConnectionRequest_Click;
            win.buttonRejectConnectionRequest.Click += ButtonRejectConnectionRequest_Click;
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

        private async void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            // Reset all user data and clear session and spot logs
            UserSession.Reset();
            SpotStore.Instance.ClearSpots();
            SessionStore.Instance.ClearSessions();
            PlannedSessionStore.Instance.ResetAll();

            // Swap grid to *login
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

                    // Call and store connection requesters >>> get list of UserConnectionDto
                    var connectionRequests = await UserConnectionService.GetConnectionRequesters();

                    List<string> requesters = new List<string>();
                    foreach (var cr in connectionRequests)
                        requesters.Add(cr.RequesterUsername);
                    UserSession.AddConnectionRequesters(requesters);                    

                    // Assign sports and fill combo box with sport modes
                    UserSession.AddSportModes(response.User.Sports);
                    win.textBlockSportMode.Text = UserSession.SelectedSportMode;
                    win.comboSportOptions.Items.Clear();
                    foreach (var sport in response.User.Sports)
                        win.comboSportOptions.Items.Add(sport);

                    AddOptionForNewSportMode();

                    win.ChangeGrid(win.Dashboard);

                    await GetUserConnections();

                    GetSpotsFromServer();
                    GetPlannedSessions();                    

                    //// Get User data from server (Admin only) ! Implement in Backend for security reasons!
                    //if (UserSession.Role == "Admin")
                    //    await UserService.GetAllUsersAsync();
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Login failed: {ex.Message}";
                MessageBox.Show(errorMessage, "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            // Open interface popup
            win.ChangePasswordPopup.IsOpen = true;
        }

        private async void ButtonConfirmPasswordChange_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(win.OldPasswordBox.Password))
                MessageBox.Show("Please enter your current password.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else if (win.NewPasswordBox.Password != win.RepeatNewPasswordBox.Password)
                MessageBox.Show("The new passwords are not the same.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                // Run service to change password
                bool passwordChanged = await UserService
                    .ChangePassword(win.OldPasswordBox.Password, win.NewPasswordBox.Password);

                if (passwordChanged)
                    win.ChangePasswordPopup.IsOpen = false;
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

        private async void ButtonSendConnectionRequest_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(win.textBoxTargetUsername.Text))
                return;

            string targetUsername = win.textBoxTargetUsername.Text.Trim();

            if (targetUsername == UserSession.Username)
            {
                MessageBox.Show("You cannot send a connection request to yourself.");
                return;
            }
            // Check if the user is already connected
            if (UserSession.IsConnectedWith(targetUsername))
            {
                MessageBox.Show($"You are already connected with {targetUsername}.");
                return;
            }

            // Check if the user has already sent a connection request
            // ...

            // Send connection request
            bool requestSent = await UserConnectionService.SendConnectionRequest(targetUsername);
            if (requestSent)
                MessageBox.Show($"Connection request sent to {targetUsername}.");
        }

        private async void ButtonAcceptConnectionRequest_Click(object sender, RoutedEventArgs e)
        {
            if (win.listConnectionRequests.SelectedItem == null)
                return;

            string selectedRequester = win.listConnectionRequests.SelectedItem.ToString()!;

            // Accept connection request
            bool connected = await UserConnectionService.AcceptConnectionRequest(selectedRequester);
            
            if (connected)
            {
                UserSession.RemoveRequesterFromList(selectedRequester);
                MessageBox.Show($"You are now connected with {selectedRequester}.");
            }
        }

        private async void ButtonRejectConnectionRequest_Click(object sender, RoutedEventArgs e)
        {
            if (win.listConnectionRequests.SelectedItem == null)
                return;

            string selectedRequester = win.listConnectionRequests.SelectedItem.ToString()!;

            // Reject connection request
            bool rejected = await UserConnectionService.RejectConnectionRequest(selectedRequester);

            if (rejected)
            {
                UserSession.RemoveRequesterFromList(selectedRequester);
                MessageBox.Show($"You have rejected the connection request from {selectedRequester}.");
            }
        }

        private void ButtonSwitchSportMode_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(win.comboSportOptions.Text))
                return;

            // Set the selected sport mode in the user session
            UserSession.SelectedSportMode = win.comboSportOptions.Text;
            win.textBlockSportMode.Text = UserSession.SelectedSportMode;

            // Send request to backend to update planned sessions for the selected sport mode
        }

        private async void ButtonAddSport_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(win.comboAddSportOptions.Text))
                return;
            
            string newSport = win.comboAddSportOptions.Text;

            // Send Put request to backend
            string mode = await UserService.SetNewSportMode(newSport);

            if (mode == newSport)
            {
                UserSession.AddSportModes(new string[] { newSport });
                MessageBox.Show($"New sport mode '{newSport}' added successfully!");
                win.comboAddSportOptions.Items.Remove(newSport);
                win.comboSportOptions.Items.Add(newSport);
            }
            else
                MessageBox.Show("Failed to add new sport mode. Please try again.");
        }

        private async Task GetUserConnections()
        {
            var friends = await UserConnectionService.GetMyConnections();

            foreach (var friend in friends)
            {
                UserSession.AddUserConnection(friend.Username, friend.Id);
                ConnectedUsersStore.Instance.AddUserConnection(friend);
            }                

            win.listConnectionRequests.ItemsSource = UserSession.ConnectionRequesters;
            win.listUserConnections.ItemsSource = UserSession.ConnectedUsersWithIDs.Keys;
        }

        private async Task GetSpotsFromServer()
        {
            var spots = await SpotService.GetSpotsAsync();
            SpotStore.Instance.SetSpots(spots);
        }

        private async Task GetPlannedSessions()
        {
            // Call server API
            SessionPlannerService.GetForeignPlannedSessions();

            // Call user's planned sessions
            SessionPlannerService.GetOwnPlannedSessions();

            // Call past planned sessions
            SessionPlannerService.GetPastPlannedSessions();
        }

        private void AddOptionForNewSportMode()
        {
            List<string> sportOptions = new List<string> { "Windsurfing", "Kitesurfing", "Wingfoiling" };

            List<string> newSpotOptionsForUser = sportOptions
                .Except(UserSession.Sports).ToList();

            win.comboAddSportOptions.Items.Clear();
            foreach (string sport in newSpotOptionsForUser)
                win.comboAddSportOptions.Items.Add(sport);
        }
    }
}
