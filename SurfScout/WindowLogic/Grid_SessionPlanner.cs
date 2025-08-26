using SurfScout.DataStores;
using SurfScout.Models;
using SurfScout.Services;
using SurfScout.SubWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        int plannedSessionId;

        public Grid_SessionPlanner(object sender, RoutedEventArgs e, MainWindow window)
        {
            this.win = window;

            SetUiInput();

            // Show popup for past unrated sessions
            win.PastSessionsPopup.IsOpen = true;
            win.buttonPastSessionsClose.Click += ButtonPastSessionsClose_Click;

            win.buttonAddPlannedSession.Click += ButtonAddPlannedSession_Click;
            win.buttonConfirmParticipation.Click += ButtonConfirmParticipation_Click;
            win.buttonConfirmNewPlannedSession.Click += ButtonConfirmNewPlannedSession_Click;
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

            if (spotId != null)
            {
                win.AddPlannedSessionPopup.IsOpen = true;
                win.datePickerNewPlanDate.DisplayDateStart = DateTime.Today;
            }
        }

        private void ButtonPastSessionsClose_Click(object sender, RoutedEventArgs e)
        {
            win.PastSessionsPopup.IsOpen = false;
        }

        private void SetUiInput()
        {
            // Set observable collections
            win.SessionPlannerListViewForeign.ItemsSource = PlannedSessionStore.Instance.PlannedSessionsForeign;
            win.SessionPlannerListViewOwn.ItemsSource = PlannedSessionStore.Instance.PlannedSessionsViewOwn;

            // Fill combobox with availaple spot names
            win.comboSpotSelector.ItemsSource = SpotStore.Instance.Spots
                                                    .Select(s => new { s.Id, s.Name })
                                                    .ToList();

            // Fill popup list of unrated sessions
            win.PastSessionsListView.ItemsSource = PlannedSessionStore.Instance.unratedSessionsViewModels;
        }

        public void ParticipateAtSession(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is int idFromButton)
            {
                this.plannedSessionId = idFromButton;

                var plannedSession = PlannedSessionStore.Instance.PlannedSessionsForeign.FirstOrDefault(s => s.Id == plannedSessionId);

                // Popup time selection for participation
                win.ParticipatePopup.IsOpen = true;
            }
        }

        public async void SkipSession(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is int idFromButton)
            {
                int skipSessionId = idFromButton;

                // Delete the user from the session
                bool removed = await SessionPlannerService.RemoveUserFromPlannedSession(skipSessionId, UserSession.UserId);

                if (removed)
                {
                    PlannedSessionStore.Instance.RemovePlannedOwnSession(skipSessionId);

                    // Update foreign sessions list
                    SessionPlannerService.GetForeignPlannedSessions();
                }                    
            }
        }

        public async void ButtonConfirmNewPlannedSession_Click(object sender, RoutedEventArgs e)
        {
            if (win.comboNewPlanStartTime.SelectedItem == null || win.comboNewPlanEndTime.SelectedItem == null)
            {
                MessageBox.Show("Please select a start and end time for the session.", "Missing Time Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TimeOnly startTime = TimeOnly.Parse((win.comboNewPlanStartTime.SelectedItem as ComboBoxItem)?.Content.ToString()!);
            TimeOnly endTime = TimeOnly.Parse((win.comboNewPlanEndTime.SelectedItem as ComboBoxItem)?.Content.ToString()!);
            DateOnly date = DateOnly.FromDateTime(win.datePickerNewPlanDate.SelectedDate!.Value);

            // TODO: Check for overlapping sessions (compare with PlannedSessionsOwn_AllSportModes)
            // ...

            // Create a new PlannedSession instance
            var newPlannedSession = new PlannedSession
            {
                Date = date,
                SpotId = (int)win.comboSpotSelector.SelectedValue,
                SportMode = UserSession.SelectedSportMode,
                Participants = new List<SessionParticipant>
                { new SessionParticipant
                    {
                        UserId = UserSession.UserId,
                        StartTime = startTime,
                        EndTime = endTime}                    
                }
            };

            // Send request to the server to create a new planned session
            var postedSessionPlan = await SessionPlannerService.PostNewSessionPlan(newPlannedSession);

            if (postedSessionPlan != null)
            {
                PlannedSessionStore.Instance.AddPlannedSessionOwn(postedSessionPlan);
                PlannedSessionStore.Instance.AddPlannedSessionsOwn_AllModes(postedSessionPlan);
                win.AddPlannedSessionPopup.IsOpen = false;
                MessageBox.Show("New planned session created successfully!", "Session Created", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show("Failed to create new planned session. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public async void ButtonConfirmParticipation_Click(object sender, RoutedEventArgs e)
        {
            if (win.comboPlannedStartTime.SelectedItem == null || win.comboPlannedEndTime.SelectedItem == null)
            {
                MessageBox.Show("Please select a start and end time for the session.", "Missing Time Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TimeOnly startTime = TimeOnly.Parse((win.comboPlannedStartTime.SelectedItem as ComboBoxItem)?.Content.ToString()!);
            TimeOnly endTime = TimeOnly.Parse((win.comboPlannedEndTime.SelectedItem as ComboBoxItem)?.Content.ToString()!);

            var participatedSession = await SessionPlannerService.ParticipateAtSession(plannedSessionId, startTime, endTime);

            if (participatedSession != null)
            {
                PlannedSessionStore.Instance.AddPlannedSessionOwn(participatedSession);
                PlannedSessionStore.Instance.RemovePlannedForeignSession(plannedSessionId);
                win.ParticipatePopup.IsOpen = false;
                MessageBox.Show("Successfully participated at the session!", "Participation Confirmed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Failed to participate at the session. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task RatePastSession(object sender, RoutedEventArgs e, int sessionId)
        {
            // Check if the sessionId is valid
            if (sessionId <= 0)
            {
                MessageBox.Show("Invalid session ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PlannedSession sessionToRate = PlannedSessionStore.Instance.PastSessions_NotRated
                .FirstOrDefault(ps => ps.Id == sessionId)!;

            if (sessionToRate == null)
                return;

            // Open the same popup as in the map viewer grid for creating a new session
            AddSessionWindow addSessionWindow = new AddSessionWindow();

            addSessionWindow.datePicker.SelectedDate = sessionToRate.Date.ToDateTime(TimeOnly.MinValue);
            addSessionWindow.datePicker.IsEnabled = false;

            addSessionWindow.comboStartTime.SelectedItem = sessionToRate.Participants
                .FirstOrDefault(p => p.Id == UserSession.UserId)?.StartTime.ToString("HH:mm");

            addSessionWindow.comboEndTime.SelectedItem = sessionToRate.Participants
                .FirstOrDefault(p => p.Id == UserSession.UserId)?.EndTime.ToString("HH:mm");

            bool? result = addSessionWindow.ShowDialog();

            if (addSessionWindow.ShowDialog() != true)
                return;

            // Get the values from the popup
            DateOnly date = addSessionWindow.date;
            TimeOnly startTime = addSessionWindow.startTime;
            TimeOnly endTime = addSessionWindow.endTime;
            string waveHeight = addSessionWindow.waveHeight;
            int rating = addSessionWindow.rating;
            double sailSize = addSessionWindow.sailSize;

            // Send request to server to store the session
            if (result.HasValue)
            {
                Session session = new Session
                {
                    Date = date,
                    Spot = SpotStore.Instance.Spots.FirstOrDefault(s => s.Id == sessionToRate.SpotId)!,
                    StartTime = startTime,
                    EndTime = endTime,
                    Wave_height = addSessionWindow.waveHeight,
                    Sail_size = addSessionWindow.sailSize,
                    Rating = addSessionWindow.rating,
                    UserId = UserSession.UserId,
                    Sport = sessionToRate.SportMode
                };

                SessionStore.Instance.AddSession(session);

                // Spot sync with server endpoint
                bool rated = await SessionService.PostSessionAsync(session);

                if (rated)
                    PlannedSessionStore.Instance.RemoveRatedOrDeletedSession(sessionId);
            }
        }

        public async Task DeletePastSession(object sender, RoutedEventArgs e, int sessionId)
        {
            // Send request to the server to remove the user from the planned session
            bool removed = await SessionPlannerService.RemoveUserFromPlannedSession(sessionId, UserSession.UserId);

            if (removed)
                PlannedSessionStore.Instance.RemoveRatedOrDeletedSession(sessionId);
        }

        public async Task ShowPlannedSessionInfo(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is int plannedSessionId)
            {
                var plannedSession = PlannedSessionStore.Instance.PlannedSessionsForeign.FirstOrDefault(s => s.Id == plannedSessionId);

                // Open popup
                // TODO: Create popup window in th UI to show session details
                // ...
            }            
        }
    }
}
