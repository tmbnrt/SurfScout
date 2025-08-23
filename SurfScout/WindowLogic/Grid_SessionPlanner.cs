using SurfScout.DataStores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SurfScout.WindowLogic
{
    class Grid_SessionPlanner
    {
        MainWindow win;

        public Grid_SessionPlanner(object sender, RoutedEventArgs e, MainWindow window)
        {
            this.win = window;

            // Set observable collections
            var plannedSessions = PlannedSessionStore.Instance.PlannedSessions
                .Select(p => new { p.Date, p.SportMode }).ToList();
            win.SessionListView.ItemsSource = plannedSessions;
        }

        public async Task ParticipateAtSession(object sender, RoutedEventArgs e)
        {
            // Popoup where the user can select the same session and can select a specific time
            // ...
        }

        public async Task ShowPlannedSessionInfo(object sender, RoutedEventArgs e)
        {
            // Popup the list where the user can see other users planning a session at a certain time
            // ...
        }
    }
}
