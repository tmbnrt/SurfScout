using SurfScout.DataStores;
using SurfScout.Models;
using SurfScout.Services;
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
        int plannedSessionId;

        public Grid_SessionPlanner(object sender, RoutedEventArgs e, MainWindow window)
        {
            this.win = window;

            SetUiInput();

            win.buttonAddPlannedSession.Click += ButtonAddPlannedSession_Click;
            win.buttonConfirmParticipation.Click += ButtonConfirmParticipation_Click;
            win.buttonConfirmNewPlannedSession.Click += ButtonConfirmNewPlannedSession_Click;
        }

        // TODO: Add function to rate a past session (create popup first)
        

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

        private void SetUiInput()
        {
            // Set observable collections
            var plannedSessions = PlannedSessionStore.Instance.PlannedSessionsForeign
                                    .Select(p => new { p.Date, p.SportMode }).ToList();
            win.SessionListView.ItemsSource = plannedSessions;

            // Fill combobox with availaple spot names
            win.comboSpotSelector.ItemsSource = SpotStore.Instance.Spots
                                                    .Select(s => new { s.Id, s.Name })
                                                    .ToList();
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

        public async void ButtonConfirmNewPlannedSession_Click(object sender, RoutedEventArgs e)
        {
            if (win.comboNewPlanStartTime.SelectedItem == null || win.comboNewPlanEndTime.SelectedItem == null)
            {
                MessageBox.Show("Please select a start and end time for the session.", "Missing Time Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TimeOnly startTime = TimeOnly.Parse((win.comboNewPlanStartTime.SelectedItem as ComboBoxItem)?.Content.ToString()!);
            TimeOnly endTime = TimeOnly.Parse((win.comboNewPlanEndTime.SelectedItem as ComboBoxItem)?.Content.ToString()!);
            DateOnly date = DateOnly.Parse(win.datePickerNewPlanDate.SelectedDate?.ToString()!);

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

        public async Task ShowPlannedSessionInfo(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is int plannedSessionId)
            {
                var plannedSession = PlannedSessionStore.Instance.PlannedSessionsForeign.FirstOrDefault(s => s.Id == plannedSessionId);

                // Open popup
                // TODO: Create popup window in th UI to show session details
            }            
        }
    }
}
