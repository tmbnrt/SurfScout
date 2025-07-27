using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Data;
using System.ComponentModel;
using NetTopologySuite.Geometries;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.Symbology;
using SurfScout.SubWindows;
using SurfScout.Services;
using SurfScout.DataStores;
using SurfScout.Models;
using SurfScout.Functions;
using System.Windows.Controls.Primitives;
using Esri.ArcGISRuntime.Location;
using SurfScout.Models.GeoModel;

namespace SurfScout.WindowLogic
{
    class Grid_MapViewer
    {
        MainWindow win;
        private static bool addSpotIsActive;
        private bool isPolygonDrawingMode;
        private List<MapPoint> savedCoordinates;
        private WindFetchPolygon polygon;
        private GraphicsOverlay polygonOverlay;

        private string mouseClick;

        // Current UI selections
        private Spot selectedSpot;
        private Session selectedSession;

        public Grid_MapViewer(object sender, RoutedEventArgs e, MainWindow window)
        {
            this.win = window;
            addSpotIsActive = false;
            this.isPolygonDrawingMode = false;
            this.savedCoordinates = new List<MapPoint>();

            LoadMap();
            Pull_SpotsFromServer();

            win.buttonMapAddSpot.Click += ButtonMapAddSpot_Click;

            // Click interaction with map
            win.SpotView.GeoViewTapped += SpotView_GeoView_Tapped;

            // Parallel check for left/right mouse klick
            win.SpotView.MouseDown += SpotView_MouseDown;

            // Click interaction with spot popup
            win.buttonCloseSpotPopup.Click += ButtonCloseSpotPopup_Click;
            win.buttonSpotAddSession.Click += ButtonSpotAddSession_Click;
            win.buttonSpotShowSessions.Click += ButtonSpotShowSessions_Click;
            win.buttonSpotRename.Click += ButtonSpotRename_Click;
            win.buttonSpotSetWindFetch.Click += ButtonSpotSetWindFetch_Click;
            win.buttonSpotShowWindFetch.Click += ButtonSpotShowWindFetch_Click;
            win.buttonSavePolygon.Click += ButtonSavePolygon_Click;
            win.buttonCancelPolygon.Click += ButtonCancelPolygon_Click;
            win.buttonClosePolygon.Click += ButtonCancelPolygon_Click;
            win.buttonShowPolygonPoints.Click += PuttonShowPolygonPoints_Click;
        }

        private void SpotView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) { }
                this.mouseClick = "left";

            if (e.ChangedButton == MouseButton.Right)
            {
                this.mouseClick = "right";
                if (isPolygonDrawingMode)
                {
                    polygon.DeleteLastPoint();
                    ShowPolygon();
                }
            }
        }

        private void ButtonCloseSpotPopup_Click(object sender, RoutedEventArgs e)
        {
            win.SpotPopup.IsOpen = false;
        }

        private void ButtonSavePolygon_Click(object sender, RoutedEventArgs e)
        {
            // Add polygon to spot instance
            SpotStore.SetWindFetchField(selectedSpot.Id, polygon.polygon);

            // Send polygon to backend
            // ...

            DeletePolygonVisualization();

            // Free polygon and close window
            ClearPolygonOverlay();
            this.polygon = null!;
            win.PolygonPopup.IsOpen = false;
            this.isPolygonDrawingMode = false;
            this.mouseClick = "";
        }

        private void ClearPolygonOverlay()
        {
            var overlay = win.SpotView.GraphicsOverlays
                .FirstOrDefault(o => o.Id == "polygonGroupOverlay");

            if (overlay != null)
            {
                var polygonsToRemove = overlay.Graphics
                    .Where(g => g.Attributes.ContainsKey("group") && g.Attributes["group"].ToString() == "polygonGroup")
                    .ToList();

                foreach (var graphic in polygonsToRemove)
                    overlay.Graphics.Remove(graphic);
            }
        }

        private void ButtonSpotShowWindFetch_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSpot.WindFetchPolygon == null)
                return;

            UI_Helpers.SetButtonState(win.buttonSavePolygon, "disable");
            win.PolygonPopup.IsOpen = true;

            // Create polygon instance from spot storage
            this.polygon = new WindFetchPolygon();
            polygon.AddExistingPolygon(selectedSpot.WindFetchPolygon);
            ShowPolygon();
        }

        private void ButtonSpotSetWindFetch_Click(object sender, RoutedEventArgs e)
        {
            // Switch to polygon drawing mode and create polygin instance
            win.PolygonPopup.IsOpen = true;
            this.isPolygonDrawingMode = true;
            UI_Helpers.SetButtonState(win.buttonSavePolygon, "disable");
            
            this.polygon = new WindFetchPolygon();
        }

        private void AddPolygonPoint(object sender, GeoViewInputEventArgs g)
        {
            MapPoint location = g.Location;

            // Convert to WGS84 (Longitude/Latitude)
            var wgsPoint = (MapPoint)GeometryEngine.Project(location, SpatialReferences.Wgs84);

            // Set point in polygon instance
            double[] p = new double[] { wgsPoint.Y, wgsPoint.X };
            polygon.SetPoint(p);

            // If last point == first point --> create json in pBuilder (precision function to target first point!!!)
            if (polygon.isClosed)
                UI_Helpers.SetButtonState(win.buttonSavePolygon, "enable");
        }

        private void ShowPolygon()
        {
            ClearPolygonOverlay();

            var polyline = new Polyline(polygon.mapPoints);

            var lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Green, 2);
            if (polygon.isClosed)
                lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Green, 4);

            var graphic = new Graphic(polyline, lineSymbol);
            graphic.Attributes["group"] = "polygonGroup";

            // Graphics overlay
            //var graphicsOverlay = new GraphicsOverlay();
            //graphicsOverlay.Id = "polygonGroupOverlay";
            polygonOverlay.Graphics.Add(graphic);
            //win.SpotView.GraphicsOverlays.Add(graphicsOverlay);
        }

        private void ButtonCancelPolygon_Click(object sender, RoutedEventArgs e)
        {
            this.isPolygonDrawingMode = false;
            this.polygon = null!;
            ClearPolygonOverlay();
            this.mouseClick = "";
            win.PolygonPopup.IsOpen = false;

            DeletePolygonVisualization();            
        }

        private async void ButtonSpotRename_Click(object sender, RoutedEventArgs e)
        {
            // Check user login and admin rights
            if (!UserSession.IsLoggedIn || !UserSession.IsAdmin)
            {
                MessageBox.Show("Only access with admin rights.", "Unauthorized");
                return;
            }

            int spotId = selectedSpot.Id;

            // Open new window
            AddSpotWindow spotNameWin = new AddSpotWindow();
            bool? result = spotNameWin.ShowDialog();

            if (result == true && !string.IsNullOrWhiteSpace(spotNameWin.SpotName))
            {
                string newSpotName = spotNameWin.SpotName;

                // Check for spots with same name
                foreach (var spot in SpotStore.Spots)
                {
                    if (spot.Name == newSpotName)
                    {
                        MessageBox.Show($"Spot {spot.Name} already available!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }

                SpotStore.RenameSpot(spotId, newSpotName);

                // Update spot name to server
                bool renameSuccessful = await SpotService.UpdateSpotNameAsync(spotId, newSpotName);

                if (renameSuccessful)
                    MessageBox.Show("Spot name successfully renamed.", "Succeeded");
                else
                    MessageBox.Show("Error trying to rename the spot.", "Failed");
            }
        }

        private async void ButtonSpotAddSession_Click(object sender, RoutedEventArgs e)
        {
            // Check user login
            if (!UserSession.IsLoggedIn)
            {
                MessageBox.Show("Please log in.", "Unauthorized");
                return;
            }

            // Open new session window
            AddSessionWindow newSessionWin = new AddSessionWindow();
            bool? result = newSessionWin.ShowDialog();

            // Create a new session and use "SessionService" to sync with backend endpoint
            if (result.HasValue && selectedSpot != null)
            {
                Session session = new Session
                {
                    Date = newSessionWin.date,
                    Spot = selectedSpot,
                    StartTime = newSessionWin.startTime,
                    EndTime = newSessionWin.endTime,
                    Wave_height = newSessionWin.waveHeight,
                    Sail_size = newSessionWin.sailSize,
                    Rating = newSessionWin.rating,
                    UserId = UserSession.UserId
                };

                SessionStore.AddSession(session);

                // Spot sync with server endpoint
                await SessionService.PostSessionAsync(session);
            }
        }

        private async void ButtonSpotShowSessions_Click(object sender, RoutedEventArgs e)
        {
            // Check user login
            if (!UserSession.IsLoggedIn)
            {
                MessageBox.Show("Please log in.", "Unauthorized");
                return;
            }

            // Pull session of the spot from api endpoint
            await SessionService.GetSessionsAsync(selectedSpot);

            // Info: "selectedSpot" is the actual selected spot in the UI
            List<Session> sessionsForSelectedSpot = SessionStore.GetSessionOfSpot(selectedSpot);

            // Mapping to create star-figures
            var sessionDisplayModels = sessionsForSelectedSpot
                .Select(s => new
                {
                    Date = s.Date.ToString("yyyy-MM-dd"),
                    Username = s.User.Username ?? "–",
                    RatingStars = UI_Helpers.GenerateStars(s.Rating)
                })
                .ToList();

            win.SessionListView.ItemsSource = sessionDisplayModels;
            win.SessionsPopup.IsOpen = true;

            // TO DO: Create button in popup grid to show selected session information (show info from backend)
            // ...
        }

        private async void LoadMap()
        {
            // Base map (WGS84 :: 4326)
            Map myMap = new Map(SpatialReference.Create(4326));

            // Shapefile path (local file)
            //string shapefilePath = @"C:\Users\Lenovo\Documents\Projekte\SurfScout\OpenStreetMap_osmdata\coastlines-split-4326\coastlines-split-4326\lines.shp";
            //string shapefilePath = @"C:\Users\Lenovo\Documents\Projekte\SurfScout\OpenStreetMap_osmdata\zuid-holland-latest-free_shp\gis_osm_water_a_free_1.shp";
            string shapefilePath = @"C:\Users\Lenovo\Documents\Projekte\SurfScout\OpenStreetMap_osmdata\COAS_RG_20M_2016_4326_shp\COAS_RG_20M_2016_4326.shp";
            ShapefileFeatureTable shapeTable = new ShapefileFeatureTable(shapefilePath);
            FeatureLayer shapeLayer = new FeatureLayer(shapeTable);

            shapeLayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Blue, 2));

            // Add layer to map
            myMap.OperationalLayers.Add(shapeLayer);
            win.SpotView.Map = myMap;

            // Add polygon overlays
            this.polygonOverlay = new GraphicsOverlay();
            this.polygonOverlay.Id = "polygonGroupOverlay";
            win.SpotView.GraphicsOverlays.Add(polygonOverlay);

            // Wait until layer has been loaded
            await shapeLayer.LoadAsync();
        }

        private async Task Pull_SpotsFromServer()
        {
            var spots = await SpotService.GetSpotsAsync();
            SpotStore.SetSpots(spots);

            // Show spots on map
            foreach (var spot in SpotStore.Spots)
            {
                if (spot.Location != null)
                    SetPin(spot.Location.Y, spot.Location.X, spot.Name, spot.Id);
            }
        }        

        private void SpotView_MouseLeftButtonDown(object sender, GeoViewInputEventArgs g)
        {
            // WebMercator coordinates to WGS84
            var mapPointOriginal = g.Location;
            var mapPoint4326 = (MapPoint)GeometryEngine.Project(mapPointOriginal, SpatialReferences.Wgs84);

            // Check for spots nearby (search distance = 300m)
            this.selectedSpot = null;
            foreach (Spot spot in SpotStore.Spots)
            {
                if (spot.CheckWithinDistance(mapPoint4326.X, mapPoint4326.Y, 300))
                    this.selectedSpot = spot;
            }

            if (selectedSpot == null)
                return;

            // Place spot popup on UI
            var screenPos = win.SpotView.LocationToScreen(mapPointOriginal);    // alternative test: mapPointOriginal

            // Enable buttons for Admin roles
            if (UserSession.IsAdmin)
            {
                win.buttonSpotSetWindFetch.IsEnabled = true;
                win.buttonSpotRename.IsEnabled = true;
            }
            else
            {
                win.buttonSpotSetWindFetch.IsEnabled = false;
                win.buttonSpotRename.IsEnabled = false;
            }

            win.buttonSpotSetWindFetch.IsEnabled = true;
            win.buttonSpotShowWindFetch.IsEnabled = true;
            win.buttonSpotRename.IsEnabled = true;

            win.PopupSpotName.Text = selectedSpot.Name;
            win.SpotPopup.Placement = PlacementMode.AbsolutePoint;
            win.SpotPopup.HorizontalOffset = screenPos.X;
            win.SpotPopup.VerticalOffset = screenPos.Y;
            win.SpotPopup.IsOpen = true;
        }

        private void OnCloseSpotPopup(object sender, RoutedEventArgs e)
        {
            win.SpotPopup.IsOpen = false;
        }

        private void ButtonMapAddSpot_Click(object sender, RoutedEventArgs e)
        {
            // Toggle new-spot-mode: on / off
            if (!addSpotIsActive)
            {
                this.win.buttonMapAddSpot.Foreground = Brushes.Goldenrod;
                addSpotIsActive = true;
            }
            else
            {
                this.win.buttonMapAddSpot.Foreground = SystemColors.ControlTextBrush;
                addSpotIsActive = false;
            }
        }

        private void SpotView_GeoView_Tapped(object sender, GeoViewInputEventArgs g)
        {
            if (!addSpotIsActive && !isPolygonDrawingMode)
                SpotView_MouseLeftButtonDown(sender, g);

            if (isPolygonDrawingMode)
            {
                if (mouseClick == "left" && !polygon.isClosed)
                    AddPolygonPoint(sender, g);

                ShowPolygon();
                return;
            }

            // Get tapped location
            MapPoint location = g.Location;

            // Convert to WGS84 (Longitude/Latitude)
            var wgsPoint = (MapPoint)GeometryEngine.Project(location, SpatialReferences.Wgs84);

            if (addSpotIsActive)
                CreateSpot(wgsPoint.Y, wgsPoint.X);
        }

        private void CreateSpot(double latitude, double longitude)
        {
            // Put pin on the map
            SetPin(latitude, longitude, "", 0);

            // Open new window
            AddSpotWindow newSpotWin = new AddSpotWindow();
            bool? result = newSpotWin.ShowDialog();

            if (result == true && !string.IsNullOrWhiteSpace(newSpotWin.SpotName))
            {
                string spotName = newSpotWin.SpotName;

                // Check for existing spots nearby
                foreach (var spot in SpotStore.Spots)
                {
                    if (spot.CheckWithinDistance(latitude, longitude, 500) || spotName == spot.Name)
                    {
                        MessageBox.Show($"Spot {spot.Name} already available!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        RemoveLastPin();
                        return;
                    }
                }

                // Remove dummy pin to create new location
                RemoveLastPin();

                // Create new spot
                var newSpotObj = new Spot
                {
                    Name = spotName,
                    Location = new NetTopologySuite.Geometries.Point(longitude, latitude),
                    Sessions = new List<Session>()
                };
                SpotStore.AddSpot(newSpotObj);

                // Pin new spot on map
                SetPin(latitude, longitude, spotName);

                // Push to server
                SpotService.SendSpotsForSyncAsync();                
            }
            else
            {
                RemoveLastPin();
                return;
            }
        }

        private void SetPin(double latitude, double longitude, string spotName, int spotId = 0)
        {
            var symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle,
                                                 System.Drawing.Color.Red,
                                                 12);
            if (spotName == "")
                symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle,
                                                 System.Drawing.Color.Black, 8);

            var point = new MapPoint(longitude, latitude, SpatialReferences.Wgs84);

            var graphic = new Graphic(point, symbol);

            // Connect spot data
            graphic.Attributes.Add("Name", spotName);
            graphic.Attributes.Add("Id", spotId);

            var pointOverlay = win.SpotView.GraphicsOverlays
                .FirstOrDefault(o => o.Id == "pointOverlay");

            if (pointOverlay == null)
            {
                pointOverlay = new GraphicsOverlay { Id = "pointOverlay" };
                win.SpotView.GraphicsOverlays.Add(pointOverlay);
            }

            pointOverlay.Graphics.Add(graphic);
        }

        private void RemoveLastPin()
        {
            var pointOverlay = win.SpotView.GraphicsOverlays
                .FirstOrDefault(o => o.Id == "pointOverlay");

            if (pointOverlay != null && pointOverlay.Graphics.Count > 0)
                pointOverlay.Graphics.RemoveAt(pointOverlay.Graphics.Count - 1);
        }

        private void DeletePolygonVisualization()
        {
            var overlay = win.SpotView.GraphicsOverlays
                .FirstOrDefault(o => o.Id == "polygonGroupOverlay");
            
            if (overlay != null)
            {
                var deletes = overlay.Graphics
                    .Where(g => g.Attributes.ContainsKey("group") && g.Attributes["group"].ToString() == "polygonGroup")
                    .ToList();
                
                foreach (var del in deletes)
                    overlay.Graphics.Remove(del);
            }
        }
    }
}
