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

namespace SurfScout.WindowLogic
{
    class WindFieldAnalyzer
    {
        private readonly MapView mapView;
        private readonly GraphicsOverlay overlay;
        private readonly MainWindow win;
        private List<WindField> windfields;
        private Session selectedSession;

        public WindFieldAnalyzer(MainWindow win, Session session)
        {
            this.win = win;
            this.mapView = win.SpotView;
            this.selectedSession = session;

            win.TimeSlider.ValueChanged += TimeSlider_ValueChanged;

            // TODO: Call wind data for the session from API
            RequestWindDataForSession();
            if (windfields == null)
                return;

            ShowTimeSlider();

            // Define new overlay
            this.overlay = new GraphicsOverlay { Id = "windFieldOverlay" };
            this.mapView.GraphicsOverlays.Add(overlay);

            // TODO: Implement butten to close/hide the wind field overlay
            // win.ButtonCloseWindField.Click += ButtonCloseWindField_Click;
        }

        private async void RequestWindDataForSession()
        {
            await SessionService.GetWindFieldDataAsync(selectedSession);
            this.windfields = SessionStore.GetWindFieldData(selectedSession.Id);
        }

        //public void ShowWindField(WindFetchPolygon polygon, WindData data)
        //{
        //    //this.overlay.Graphics.Clear();
        //    //
        //    //var symbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.FromArgb(100, 0, 0, 255), null);
        //    //var graphic = new Graphic(polygon.polygon, symbol);
        //    //this.overlay.Graphics.Add(graphic);
        //    //
        //    //// Further wind analyes ...
        //}

        private void ShowTimeSlider()
        {
            win.TimeSlider.Visibility = Visibility.Collapsed;
            win.TimeSlider.Minimum = windfields.First().Timestamp.Hour;
            win.TimeSlider.MaxHeight = windfields.Last().Timestamp.Hour;

            // Set tick frequency: time difference between first and second windfield in hours as integer
            int timeDifference = (int)(windfields[1].Timestamp - windfields[0].Timestamp).TotalHours;
            win.TimeSlider.TickFrequency = timeDifference;
        }

        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int hour = (int)e.NewValue;

            // Show timestamp
            Debug.WriteLine($"Local time: {hour}:00");

            // TODO: Update the wind field graphics based on the selected hour (INTERPOLATION FIRST)
            // ...
        }

        public void ClearWindField()
        {
            win.TimeSlider.Visibility = Visibility.Collapsed;
            this.overlay.Graphics.Clear();
        }
    }
}
