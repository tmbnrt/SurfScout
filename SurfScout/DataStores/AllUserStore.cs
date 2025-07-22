using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurfScout.Models;

namespace SurfScout.DataStores
{
    // This class is the representation of all registered user
    public class AllUserStore
    {
        private static List<User> _users = new();
        public static IReadOnlyList<User> Users => _users;

        public static void GetUsers(IEnumerable<User> users)
        {
            _users = new List<User>(users);
        }

        public static string GetUserName(int id)
        {
            foreach (User u in _users)
                if (u.Id == id)
                    return u.Username;

            return "-unknown-";
        }

        public static void SetAllUsers(IEnumerable<User> users)
        {
            _users = new List<User>(users);
        }

        // Method delete user --> API function     ** for admin role only **
        // ...
    }
}
