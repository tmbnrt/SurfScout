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
        }

        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = win.UsernameBox.Text.Trim();
            string password = win.PasswordBox.Password;

            // API login
            var service = new UserService();
            var (success, token, user) = await service.LoginAsync(username, password);

            if (success)
            {
                UserSession.jwtToken = token;
                UserSession.username = username;

                MessageBox.Show($"Login successful. Welcome back, {user}!");
                win.UsernameBox.Text = string.Empty;
                win.PasswordBox.Password = string.Empty;

                // Change button name to user name
                win.buttonUserName.Text = username;
            }

            // if login successfull, change content of "UserLogin botton" to the name of the user
            // ...

            // swap content of the "userLogin"-grid to "userInfo"-grid
            // ...

            // if logout swap back to "userLogin"-grid
            // ...
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
