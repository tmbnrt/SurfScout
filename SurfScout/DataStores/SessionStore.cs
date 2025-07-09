using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SurfScout.Models;

namespace SurfScout.DataStores
{
    public static class SessionStore
    {
        private static List<Session> _sessions = new();
        public static IReadOnlyList<Session> Sessions => _sessions;

        public static void SetSessions(IEnumerable<Session> sessions)
        {
            _sessions = new List<Session>(sessions);
        }

        public static void AddSession(Session session)
        {
            _sessions.Add(session);
        }

        public static void RemoveSession(Session session)
        {
            _sessions.Remove(session);
        }

        public static void ClearSessions()
        {
            _sessions.Clear();
        }
    }
}
