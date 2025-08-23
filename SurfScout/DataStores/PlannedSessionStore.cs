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

        public ObservableCollection<PlannedSession> PlannedSessions { get; set; } = new ObservableCollection<PlannedSession>();

        public void AddPlannedSession(PlannedSession plannedSession)
        {
            PlannedSessions.Add(plannedSession);
        }

        public void RemoveSession(PlannedSession plannedSession)
        {
            // Check if user is the only participant
            // ... if yes, delete session

            // else remove session for the user and also update database to remove the user (by id)
            // ...
        }

        // observable collection provider for "SessionPlannerListView"
    }
}
