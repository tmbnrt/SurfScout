using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SurfScout.Models;
using SurfScout.Models.DTOs;
using SurfScout.Models.WindModel;
using SurfScout.Services;

namespace SurfScout.DataStores
{
    public class SessionStore
    {
        private static SessionStore _instance;
        public static SessionStore Instance => _instance ??= new SessionStore();

        private List<Session> _sessions = new();
        public IReadOnlyList<Session> Sessions => _sessions;

        public void SetSessions(IEnumerable<Session> sessions)
        {
            _sessions = new List<Session>(sessions);
        }

        public void AddSession(Session session)
        {
            _sessions.Add(session);
        }

        public void RemoveSession(Session session)
        {
            _sessions.Remove(session);
        }

        public void ClearSessions()
        {
            _sessions.Clear();
        }

        public List<Session> GetSessionOfSpot(Spot spot)
        {
            List<Session> sessionList = new List<Session>();

            foreach (Session s in _sessions)
                if (s.Spot == spot && s.Sport == UserSession.SelectedSportMode)
                    sessionList.Add(s);

            return sessionList;
        }

        public Session GetSessionById(int id)
        {
            foreach (Session session in _sessions)
                if (session.Id == id)
                    return session;

            return null!;
        }

        public void PutWindFieldData(int id, List<WindField> windfields)
        {
            foreach (Session session in _sessions)
                if (session.Id == id)
                    session.WindFields = windfields;
        }

        public List<WindField> GetWindFieldData(int id)
        {
            foreach (Session session in _sessions)
                if (session.Id == id)
                {
                    if (session.WindFields != null)
                        return session.WindFields;
                    else
                        return new List<WindField>();
                }

            return null!;
        }
    }
}
