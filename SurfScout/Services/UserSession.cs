using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfScout.Services
{
    public static class UserSession
    {
        public static string jwtToken { get; set; }
        public static string username { get; set; }
        public static int userId { get; set; }

    }
}
