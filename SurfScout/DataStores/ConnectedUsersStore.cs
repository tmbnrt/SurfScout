using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurfScout.Models;
using SurfScout.Models.DTOs;

namespace SurfScout.DataStores
{
    class ConnectedUsersStore
    {
        private static ConnectedUsersStore _instance;
        public static ConnectedUsersStore Instance => _instance ??= new ConnectedUsersStore();

        public List<User> UserConnections { get; set; } = new List<User>();

        public void AddUserConnection(User user)
        {
            if (UserConnections.Any(u => u.Id == user.Id))
                return;

            UserConnections.Add(user);
        }

        public string GetUsernameById(int id)
        {
            var user = UserConnections.FirstOrDefault(u => u.Id == id);
            return user != null ? user.Username : null;
        }


        // List of <User>

    }
}
