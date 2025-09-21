using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Symbology;
using SurfScout.Models.GeoModel;
using SurfScout.Models;
using SurfScout.Models.WindModel;
using SurfScout.DataStores;
using SurfScout.Services;
using SurfScout.Functions.GeoFunctions;
using SurfScout.Functions.GraphicsFunctions;
using System.Windows;
using System.Diagnostics;
using Windows.UI.Input.Inking.Analysis;
using Esri.ArcGISRuntime.Geometry;
using System.Drawing;

namespace SurfScout.WindowLogic
{
    class WindFieldVisualizer
    {
        private readonly MapView mapView;
        private GraphicsOverlay overlay_windDirectionMarkers;
        private GraphicsOverlay overlay_interpolated;
        private Dictionary<int, GraphicsOverlay> overlays_windmarkers_by_hours;
        private Dictionary<int, GraphicsOverlay> overlays_interpolated_by_hours;
        private readonly MainWindow win;
        private List<WindField> windfieldmeasures;
        private List<WindFieldInterpolated> windfieldinterpolated;
        private Session selectedSession;
        private Spot selectedSpot;

        public WindFieldVisualizer(MainWindow win, Session session, Spot spot)
        {
            this.win = win;
            this.mapView = win.SpotView;
            this.selectedSession = session;
            this.selectedSpot = spot;

            win.TimeSlider.ValueChanged += TimeSlider_ValueChanged;
        }

        public async Task RequestWindDataForSession()
        {
            await WindFieldService.GetWindFieldMeasureDataAsync(selectedSession);
            await WindFieldService.GetInterpolatedWindFieldDataAsync(selectedSession);
            await SpotService.GetWindFetchArea(selectedSpot.Id);

            this.windfieldmeasures = SessionStore.Instance.GetWindFieldData(selectedSession.Id);
            this.windfieldinterpolated = SessionStore.Instance.GetWindFieldInterpolated(selectedSession.Id);

            if (windfieldmeasures == null || windfieldinterpolated == null || selectedSpot.WindFetchPolygon == null)
                return;

            // Create overlays from interpolated API data (geojson/gzip)
            this.overlays_interpolated_by_hours = OverlayFunctions
                                                    .CreateInterpolatedWindFieldOverlays(windfieldinterpolated);

            ShowTimeSlider();
        }

        // Method to create direction markers from measure field only
        private async Task CreateWindspeedMarkerOverlays()
        {
            this.overlays_windmarkers_by_hours = new Dictionary<int, GraphicsOverlay>();

            foreach (WindField wfm in windfieldmeasures)
            {
                GraphicsOverlay overlay = OverlayFunctions
                                            .CreateGraphicsOverlayWindmarkers_Old(wfm, selectedSpot.WindFetchPolygon!);
                if (overlay != null)
                    this.overlays_windmarkers_by_hours.Add(wfm.Timestamp.Hour, overlay);
            }
        }

        // Local interpolation not used anymore as interpolated data can be called from the API
        private async Task CreateInterpolatedWindspeedOverlays()
        {
            this.overlays_interpolated_by_hours = new Dictionary<int, GraphicsOverlay>();

            foreach (WindField wf in windfieldmeasures)
            {
                GraphicsOverlay overlay = SpatialOperations
                                            .CreateGraphicsOverlayInterpolationIDW(wf,
                                                                                selectedSpot.WindFetchPolygon!,
                                                                                1000);
                if (overlay != null)
                    this.overlays_interpolated_by_hours.Add(wf.Timestamp.Hour, overlay);
            }
        }

        private void ShowWindField(int hour)
        {
            if (overlay_windDirectionMarkers != null)
                this.overlay_windDirectionMarkers.Graphics.Clear();

            if (selectedSpot.WindFetchPolygon == null)
                return;

            this.overlay_windDirectionMarkers = OverlayFunctions.CreateWindDirectionMarkers(windfieldmeasures, hour);
            if (overlay_windDirectionMarkers == null)
                return;
                        
            this.mapView.GraphicsOverlays.Add(overlay_windDirectionMarkers);

            // Add interpolated overlay for the current hour
            if (mapView.GraphicsOverlays.Contains(overlays_interpolated_by_hours[hour]))
                this.mapView.GraphicsOverlays.Remove(overlays_interpolated_by_hours[hour]);
            if (overlays_interpolated_by_hours.ContainsKey(hour))
                this.mapView.GraphicsOverlays.Add(overlays_interpolated_by_hours[hour]);
        }
        
        private void ShowTimeSlider()
        {
            win.TimeSlider.Visibility = Visibility.Visible;
            win.TimeSlider.Minimum = windfieldmeasures.First().Timestamp.Hour;
            win.TimeSlider.Maximum = windfieldmeasures.Last().Timestamp.Hour;

            // Set tick frequency: time difference between first and second windfield in hours as integer
            int timeDifference = (int)(windfieldmeasures[1].Timestamp - windfieldmeasures[0].Timestamp).TotalHours;
            win.TimeSlider.TickFrequency = timeDifference;
        }

        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int hour = (int)e.NewValue;

            // Update the wind field graphics based on the selected hour
            ShowWindField(hour);
        }

        public void ClearWindField()
        {
            win.TimeSlider.Visibility = Visibility.Collapsed;
            this.overlay_windDirectionMarkers.Graphics.Clear();
            this.overlay_interpolated.Graphics.Clear();
        }
    }
}
