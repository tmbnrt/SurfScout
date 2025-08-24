using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurfScout.Models;
using SurfScout.Models.ViewModel;
using SurfScout.Services;

namespace SurfScout.DataStores
{
    public class PlannedSessionStore
    {
        private static PlannedSessionStore _instance;
        public static PlannedSessionStore Instance => _instance ??= new PlannedSessionStore();

        public ObservableCollection<PlannedSession> PlannedSessionsForeign { get; set; } = new ObservableCollection<PlannedSession>();
        public ObservableCollection<PlannedSession> PlannedSessionsOwn { get; set; } = new ObservableCollection<PlannedSession>();
        public List<PlannedSession> PlannedSessionsOwn_AllSportModes { get; set; } = new List<PlannedSession>();
        public List<PlannedSession> PastSessions_NotRated { get; set; } = new List<PlannedSession>();
        public ObservableCollection<UnratedSessionsViewModel> unratedSessionsViewModels { get; set; } = new ObservableCollection<UnratedSessionsViewModel>();

        public void AddPlannedSessionForeign(PlannedSession plannedSession)
        {
            plannedSession.SpotName = SpotStore.Instance.Spots
                .FirstOrDefault(s => s.Id == plannedSession.SpotId)?.Name ?? "Unknown";
            PlannedSessionsForeign.Add(plannedSession);
        }

        public void AddPlannedSessionOwn(PlannedSession plannedSession)
        {
            PlannedSessionsOwn.Add(plannedSession);
        }

        public void AddPastSession(PlannedSession plannedSession)
        {
            PastSessions_NotRated.Add(plannedSession);

            // Add to the unrated sessions view model for displaying in the UI
            unratedSessionsViewModels.Add(new UnratedSessionsViewModel
            {
                Id = plannedSession.Id,
                Date = plannedSession.Date,
                SpotName = SpotStore.Instance.Spots
                    .FirstOrDefault(s => s.Id == plannedSession.SpotId)?.Name ?? "Unknown",
                StartTime = plannedSession.Participants
                    .FirstOrDefault(p => p.Id == UserSession.UserId)!.StartTime,
                EndTime = plannedSession.Participants
                    .FirstOrDefault(p => p.Id == UserSession.UserId)!.EndTime,
                SportMode = plannedSession.SportMode
            });
        }

        public void AddPlannedSessionsOwn_AllModes(PlannedSession plannedSession)
        {
            PlannedSessionsOwn_AllSportModes.Add(plannedSession);
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
        public void RemoveRatedOrDeletedSession(int sessionId)
        {
            var sessionToRemove = PastSessions_NotRated.FirstOrDefault(s => s.Id == sessionId);
            if (sessionToRemove != null)
                PastSessions_NotRated.Remove(sessionToRemove);

            // Also remove from unrated sessions view model
            var unratedSessionToRemove = unratedSessionsViewModels.FirstOrDefault(s => s.Id == sessionId);
            if (unratedSessionToRemove != null)
                unratedSessionsViewModels.Remove(unratedSessionToRemove);
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
