using SurfScout.DataStores;
using SurfScout.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SurfScout.WindowLogic
{
    class Grid_SessionPlanner
    {
        MainWindow win;

        public Grid_SessionPlanner(object sender, RoutedEventArgs e, MainWindow window)
        {
            this.win = window;

            SetUiInput();

            win.buttonAddPlannedSession.Click += ButtonAddPlannedSession_Click;
        }

        private void ButtonAddPlannedSession_Click(object sender, RoutedEventArgs e)
        {
            var selected = win.comboSpotSelector.SelectedItem;
            if (selected == null)
            {
                MessageBox.Show("Please select a spot to plan a session.", "No Spot Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int spotId = (int)win.comboSpotSelector.SelectedValue;
            var spotName = selected.GetType().GetProperty("Name")?.GetValue(selected)?.ToString();

            // Open a popup to set more details about the session (date, time, sportmode)
            // TODO: Create popup in UI ...

            // --> comboSpotSelector shows the spot name but spot id is also contained in the object
        }

        private void SetUiInput()
        {
            // Set observable collections
            var plannedSessions = PlannedSessionStore.Instance.PlannedSessions
                                    .Select(p => new { p.Date, p.SportMode }).ToList();
            win.SessionListView.ItemsSource = plannedSessions;

            // Fill combobox with availaple spot names
            win.comboSpotSelector.ItemsSource = SpotStore.Instance.Spots
                                                    .Select(s => new { s.Id, s.Name })
                                                    .ToList();
        }

        public async Task ParticipateAtSession(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is int plannedSessionId)
            {
                var plannedSession = PlannedSessionStore.Instance.PlannedSessions.FirstOrDefault(s => s.Id == plannedSessionId);

                // Send put request to the server to add the user to the planned session
                // TODO: Implement API call to add user to the session ...
            }
        }

        public async Task ShowPlannedSessionInfo(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is int plannedSessionId)
            {
                var plannedSession = PlannedSessionStore.Instance.PlannedSessions.FirstOrDefault(s => s.Id == plannedSessionId);

                // Open popup
                // TODO: Create popup window in th UI to show session details
            }            
        }
    }
}
