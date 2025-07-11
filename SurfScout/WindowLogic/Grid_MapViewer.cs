﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
using System.Windows.Shapes;
using System.Data;
using System.IO;
using System.ComponentModel;
using NetTopologySuite.Geometries;
using Esri.ArcGISRuntime;
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

namespace SurfScout.WindowLogic
{
    class Grid_MapViewer
    {
        MainWindow win;
        private static bool addSpotIsActive;
        private static bool addSessionIsActive;
        private List<MapPoint> savedCoordinates;

        public Grid_MapViewer(object sender, RoutedEventArgs e, MainWindow window)
        {
            this.win = window;
            addSpotIsActive = false;
            addSessionIsActive = false;
            this.savedCoordinates = new List<MapPoint>();

            LoadMap();

            // Pull spots with location from server (stroe in SpotStore) and place on the map
            Pull_SpotsFromServer();

            win.buttonMapAddSpot.Click += buttonMapAddSpot_Click;
            win.buttonMapAddSession.Click += buttonMapAddSession_Click;

            // Click interaction
            win.SpotView.GeoViewTapped += SpotView_GeoView_Tapped;
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
                if (spot.location != null)
                    SetPin(spot.location.Y, spot.location.X, spot.name, spot.id);
            }
        }

        private void MyMapView_GeoView_Click(object sender, GeoViewInputEventArgs e)
        {
            MapPoint clickedPoint = e.Location;
            savedCoordinates.Add(clickedPoint);
            Console.WriteLine($"Clicked coordinates: {clickedPoint.X}, {clickedPoint.Y}");
        }

        private void buttonMapAddSpot_Click(object sender, RoutedEventArgs e)
        {
            addSessionIsActive = false;
            this.win.buttonMapAddSession.Foreground = SystemColors.ControlTextBrush;

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

        private void buttonMapAddSession_Click(object sender, RoutedEventArgs e)
        {
            addSpotIsActive = false;
            this.win.buttonMapAddSpot.Foreground = SystemColors.ControlTextBrush;

            // Toggle new-spot-mode: on / off
            if (!addSessionIsActive)
            {
                this.win.buttonMapAddSession.Foreground = Brushes.Goldenrod;
                addSessionIsActive = true;
            }
            else
            {
                this.win.buttonMapAddSession.Foreground = SystemColors.ControlTextBrush;
                addSessionIsActive = false;
            }
        }

        private void SpotView_GeoView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            if (!addSpotIsActive && !addSessionIsActive)
                return;

            // Get tapped location
            MapPoint location = e.Location;

            // Convert to WGS84 (Longitude/Latitude)
            var wgsPoint = (MapPoint)GeometryEngine.Project(location, SpatialReferences.Wgs84);

            if (addSpotIsActive)
                CreateSpot(wgsPoint.Y, wgsPoint.X);

            //if (addSessionIsActive)
            // ...
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
                    if (spot.CheckWithinDistance(latitude, longitude, 500) || spotName == spot.name)
                    {
                        MessageBox.Show($"Spot {spot.name} already available!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        RemoveLastPin();
                        return;
                    }
                }

                RemoveLastPin();    // dummy pin to create new location

                // Create new spot
                var newSpotObj = new Spot
                {
                    name = spotName,
                    location = new NetTopologySuite.Geometries.Point(longitude, latitude),
                    sessions = new List<Session>()
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
                                                 12); // Size of the pin
            if (spotName == "")
                symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle,
                                                 System.Drawing.Color.Black, 8);

            var point = new MapPoint(longitude, latitude, SpatialReferences.Wgs84);

            var graphic = new Graphic(point, symbol);

            // Connect spot data
            graphic.Attributes.Add("Name", spotName);
            graphic.Attributes.Add("Id", spotId);

            if (win.SpotView.GraphicsOverlays.Count == 0)
                win.SpotView.GraphicsOverlays.Add(new GraphicsOverlay());

            var overlay = win.SpotView.GraphicsOverlays[0];

            overlay.Graphics.Add(graphic);
        }

        private void RemoveLastPin()
        {
            if (win.SpotView.GraphicsOverlays.Count > 0)
            {
                var graphics = win.SpotView.GraphicsOverlays[0].Graphics;
                if (graphics.Count > 0)
                    graphics.RemoveAt(graphics.Count - 1);
            }
        }
    }
}
