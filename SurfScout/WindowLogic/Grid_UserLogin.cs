using SurfScout.DataStores;
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

            win.buttonLogin.Click += ButtonLogin_Click;
            win.buttonRegister.Click += ButtonRegister_Click;
            win.buttonLogout.Click += ButtonLogout_Click;
        }

        private async void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            // Reset all user data and clear session and spot logs
            UserSession.Reset();
            SpotStore.ClearSpots();
            SessionStore.ClearSessions();

            // If succeeded -> swap grid to *login
            win.buttonUserName.Text = "User Login";
            win.ChangeGrid(win.UserLogin);
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
                if (response.Success)
                {
                    UserSession.JwtToken = response.Token;
                    UserSession.Username = response.User.Username;
                    UserSession.UserId = response.User.Id;

                    MessageBox.Show($"Login successful. Welcome back, {username}!");
                    win.UsernameBox.Text = string.Empty;
                    win.PasswordBox.Password = string.Empty;

                    // Store user info
                    //UserStore.Role

                    // Change button name to user name and swap grid from *login to *userinfo
                    win.buttonUserName.Text = response.User.Username;
                    win.LoggedUser.Text = response.User.Username;
                    win.ChangeGrid(win.UserInfo);

                    // Get User data from server
                    // ... Fill "AllUserStore"
                    // ...
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

            // API Registration
            var service = new UserService();
            if (await service.RegisterUserAsync(username, password))
            {
                MessageBox.Show("Registration successful. Please log in.");
                win.UsernameBox = null;
                win.PasswordBox = null;
            }
        }
    }
}
