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
using System.Windows;
using System.Diagnostics;
using Windows.UI.Input.Inking.Analysis;
using Esri.ArcGISRuntime.Geometry;
using System.Drawing;

namespace SurfScout.WindowLogic
{
    class WindFieldAnalyzer
    {
        private readonly MapView mapView;
        private GraphicsOverlay overlay_arrows;
        private GraphicsOverlay overlay_interpolated;
        private readonly MainWindow win;
        private List<WindField> windfields;
        private Session selectedSession;

        public WindFieldAnalyzer(MainWindow win, Session session)
        {
            this.win = win;
            this.mapView = win.SpotView;
            this.selectedSession = session;

            win.TimeSlider.ValueChanged += TimeSlider_ValueChanged;
        }

        public async Task RequestWindDataForSession()
        {
            await SessionService.GetWindFieldDataAsync(selectedSession);
            this.windfields = SessionStore.GetWindFieldData(selectedSession.Id);

            if (windfields == null)
                return;

            ShowTimeSlider();
        }

        private void ShowWindField(int hour)
        {
            if (overlay_arrows != null)
                this.overlay_arrows.Graphics.Clear();

            this.overlay_arrows = new GraphicsOverlay();

            // Get wind field for selected time
            WindField currentWindField = windfields.FirstOrDefault(wf => wf.Timestamp.Hour == hour);
            if (currentWindField == null)
                return;

            foreach (WindFieldPoint wfp in currentWindField.Points)
            {
                // Convert from Nettopology in esri point
                var location = new Esri.ArcGISRuntime.Geometry.MapPoint(wfp.Location.X, wfp.Location.Y, SpatialReferences.Wgs84);

                var arrowSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, System.Drawing.Color.Blue, wfp.WindSpeedKnots);
                double size = Math.Clamp(wfp.WindSpeedKnots, 5, 50);
                arrowSymbol.Size = size;

                // Add 180 degree to the angle but result is in range 0-360
                double angle = wfp.WindDirectionDegree + 180;
                if (angle >= 360)
                    angle -= 360;

                arrowSymbol.Angle = angle;

                var graphic = new Graphic(location, arrowSymbol);
                overlay_arrows.Graphics.Add(graphic);
            }
            
            this.mapView.GraphicsOverlays.Add(overlay_arrows);
        }
        
        private void ShowTimeSlider()
        {
            win.TimeSlider.Visibility = Visibility.Visible;
            win.TimeSlider.Minimum = windfields.First().Timestamp.Hour;
            win.TimeSlider.Maximum = windfields.Last().Timestamp.Hour;

            // Set tick frequency: time difference between first and second windfield in hours as integer
            int timeDifference = (int)(windfields[1].Timestamp - windfields[0].Timestamp).TotalHours;
            win.TimeSlider.TickFrequency = timeDifference;
        }

        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int hour = (int)e.NewValue;

            // TODO: Update the wind field graphics based on the selected hour (INTERPOLATION FIRST)
            ShowWindField(hour);
        }

        public void ClearWindField()
        {
            win.TimeSlider.Visibility = Visibility.Collapsed;
            this.overlay_arrows.Graphics.Clear();
            this.overlay_interpolated.Graphics.Clear();
        }
    }
}
