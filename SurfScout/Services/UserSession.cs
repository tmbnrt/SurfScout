using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfScout.Services
{
    public static class UserSession
    {
        public static int UserId { get; set; }
        public static string Username { get; set; }
        public static string Role { get; set; }
        public static string JwtToken { get; set; }
        public static bool IsLoggedIn => !string.IsNullOrEmpty(JwtToken);
        public static bool IsAdmin => Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

        public static void Reset()
        {
            UserId = 0;
            Username = null;
            Role = null;
            JwtToken = null;
        }
    }
}
