using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
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
        public static List<string> Sports { get; set; } = new List<string>();
        public static Dictionary<string, int> ConnectedUsersWithIDs { get; set; } = new Dictionary<string, int>();
        public static ObservableCollection<string> ConnectionRequesters { get; set; } = new ObservableCollection<string>();

        public static void Reset()
        {
            UserId = 0;
            Username = null;
            Role = null;
            JwtToken = null;
            SelectedSportMode = "...";
            Sports = new List<string>();
            ConnectedUsersWithIDs = new Dictionary<string, int>();
            ConnectionRequesters = new ObservableCollection<string>();
        }

        public static void SetSportMode(string sportMode)
        {
            SelectedSportMode = sportMode;
        }

        public static void AddSportModes(string[] sportModes)
        {
            Sports = sportModes.ToList();
            SetSportMode(Sports.FirstOrDefault() ?? "...");
        }

        public static bool IsConnectedWith(string name)
        {
            if (ConnectedUsersWithIDs.ContainsKey(name))
                return true;
            else
                return false;
        }

        public static void AddConnectionRequesters(List<string> requesters)
        {
            if (requesters == null)
                return;
            else
            {
                foreach (string name in requesters)
                    ConnectionRequesters.Add(name);
            }
        }

        public static void AddUserConnection(string name, int id)
        {
            ConnectedUsersWithIDs.Add(name, id);
        }

        public static void DeleteUserConnection(string name)
        {
            ConnectedUsersWithIDs.Remove(name);
        }

        public static void RemoveRequesterFromList(string requester)
        {
            if (ConnectionRequesters.Contains(requester))
                ConnectionRequesters.Remove(requester);
        }
    }
}
