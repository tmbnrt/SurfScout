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
        public static string SelectedSportMode { get; set; } = "...";
        public static List<string> connectedUsers { get; set; } = new List<string>();
        public static List<string> connectionRequesters { get; set; } = new List<string>();

        public static void Reset()
        {
            UserId = 0;
            Username = null;
            Role = null;
            JwtToken = null;
        }

        public static void SetSportMode(string sportMode)
        {
            SelectedSportMode = sportMode;
        }

        public static void AddConnectionRequesters(List<string> requesters)
        {
            if (requesters == null)
                return;
            else
                connectionRequesters.AddRange(requesters);
        }

        //public static void UpdateUserConnections(List<User> connections)
        //{
        //
        //}

        public static void DeleteRequesterFromList(string requester)
        {
            if (connectionRequesters.Contains(requester))
                connectionRequesters.Remove(requester);
        }
    }
}
