using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurfScout.Models;

namespace SurfScout.DataStores
{
    public class PlannedSessionStore
    {
        private static PlannedSessionStore _instance;
        public static PlannedSessionStore Instance => _instance ??= new PlannedSessionStore();

        public ObservableCollection<PlannedSession> PlannedSessionsForeign { get; set; } = new ObservableCollection<PlannedSession>();
        public ObservableCollection<PlannedSession> PlannedSessionsOwn { get; set; } = new ObservableCollection<PlannedSession>();
        public List<PlannedSession> PlannedSessionsOwn_AllSportModes { get; set; } = new List<PlannedSession>();
        public ObservableCollection<PlannedSession> PastSessions_NotRated { get; set; } = new ObservableCollection<PlannedSession>();

        public void AddPlannedSessionForeign(PlannedSession plannedSession)
        {
            PlannedSessionsForeign.Add(plannedSession);
        }

        public void AddPlannedSessionOwn(PlannedSession plannedSession)
        {
            PlannedSessionsOwn.Add(plannedSession);
        }

        public void AddPlannedSessionsOwn_AllModes(PlannedSession plannedSession)
        {
            PlannedSessionsOwn_AllSportModes.AddRange(PlannedSessionsOwn);
        }

        public void RemovePlannedForeignSession(int sessionId)
        {
            var sessionToRemove = PlannedSessionsForeign.FirstOrDefault(s => s.Id == sessionId);
            if (sessionToRemove != null)
                PlannedSessionsForeign.Remove(sessionToRemove);
        }

        public void RemovePlannedOwnSession(int sessionId)
        {
            var sessionToRemove = PlannedSessionsOwn.FirstOrDefault(s => s.Id == sessionId);
            if (sessionToRemove != null)
                PlannedSessionsOwn.Remove(sessionToRemove);
        }

        // Function to check for past planned session in the own collection. If in the past, delete it and add to PastSessions_NotRated
        public void CheckAndMovePastSessions()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var pastSessions = PlannedSessionsOwn.Where(s => s.Date < today).ToList();
            foreach (var session in pastSessions)
            {
                PlannedSessionsOwn.Remove(session);
                PastSessions_NotRated.Add(session);
            }
        }

        // Function to delete session because it got rated by the user
        public void DeleteRatedSession(int sessionId)
        {
            var sessionToRemove = PastSessions_NotRated.FirstOrDefault(s => s.Id == sessionId);
            if (sessionToRemove != null)
                PastSessions_NotRated.Remove(sessionToRemove);
        }

        public void ResetAll()
        {
            PlannedSessionsForeign.Clear();
            PlannedSessionsOwn.Clear();
            PlannedSessionsOwn_AllSportModes.Clear();
            PastSessions_NotRated.Clear();
        }
    }
}
