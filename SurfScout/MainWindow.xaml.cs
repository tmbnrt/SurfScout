using System;
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
using System.Security.Cryptography.X509Certificates;
using SurfScout.WindowLogic;
using Esri.ArcGISRuntime.UI.Controls;
using SurfScout.Services;


namespace SurfScout;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private List<Button> buttons;
    private List<Grid> grids;
    public bool eventHandlerIsAttached_Grid_Dashboard = false;
    public bool eventHandlerIsAttached_Grid_MapViewer = false;
    public bool eventHandlerIsAttached_Grid_WindAnalytics = false;
    public bool eventHandlerIsAttached_Grid_ForecastAnalysis = false;
    public bool eventHandlerIsAttached_Grid_SessionPlanner = false;
    public bool eventHandlerIsAttached_Grid_Forum = false;

    private Grid_MapViewer? grid_mapviever;

    public MainWindow()
    {
        InitializeComponent();

        // Maximized window
        WindowState = WindowState.Maximized;

        AddButtons();
        AddGrids();

        // Button interaction
        buttonDashboard.Click += buttonDashboard_Click;
        buttonMapViewer.Click += ButtonMapViewer_Click;
        buttonWindAnalytics.Click += buttonWindAnalytics_Click;
        buttonForecastAnalysis.Click += buttonForecastAnalysis_Click;
        buttonSessionPlanner.Click += ButtonSessionPlanner_Click;
        buttonForum.Click += buttonForum_Click;        
    }

    private void ButtonSessionPlanner_Click(object sender, RoutedEventArgs e)
    {
        ChangeColor(buttonSessionPlanner);
        ChangeGrid(SessionPlanner);

        if (!eventHandlerIsAttached_Grid_SessionPlanner)
        {
            Grid_SessionPlanner grid_sessionplanner = new Grid_SessionPlanner(sender, e, this);
            eventHandlerIsAttached_Grid_SessionPlanner = true;
        }
    }

    private void MenuToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        MenuOverlay.Visibility = Visibility.Visible;
    }

    private void MenuToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        MenuOverlay.Visibility = Visibility.Collapsed;
    }

    private void buttonDashboard_Click(object sender, RoutedEventArgs e)
    {
        ChangeColor(buttonDashboard);
        ChangeGrid(Dashboard);

        if (!eventHandlerIsAttached_Grid_Dashboard)
        {
            Grid_Dashboard grid_dashboard = new Grid_Dashboard(sender, e, this);
            eventHandlerIsAttached_Grid_Dashboard = true;
        }
    }

    private void ButtonMapViewer_Click(object sender, RoutedEventArgs e)
    {
        ChangeColor(buttonMapViewer);
        ChangeGrid(MapViewer);

        if (!eventHandlerIsAttached_Grid_MapViewer)
        {
            this.grid_mapviever = new Grid_MapViewer(sender, e, this);
            eventHandlerIsAttached_Grid_MapViewer = true;
        }
    }

    private void buttonForum_Click(object sender, RoutedEventArgs e)
    {
        ChangeColor(buttonForum);
        ChangeGrid(Forum);

        if (!eventHandlerIsAttached_Grid_Forum)
        {
            Grid_Forum grid_forum = new Grid_Forum(sender, e, this);
            eventHandlerIsAttached_Grid_Forum = true;
        }
    }

    private void buttonForecastAnalysis_Click(object sender, RoutedEventArgs e)
    {
        ChangeColor(buttonForecastAnalysis);
        ChangeGrid(ForecastAnalysis);
        if (!eventHandlerIsAttached_Grid_ForecastAnalysis)
        {
            //Grid_ForecastAnalysis grid_forecastanalysis = new Grid_ForecastAnalysis(sender, e, this);
            eventHandlerIsAttached_Grid_ForecastAnalysis = true;
        }
    }

    private void buttonWindAnalytics_Click(object sender, RoutedEventArgs e)
    {
        ChangeColor(buttonWindAnalytics);
        ChangeGrid(WindAnalysis);

        if (!eventHandlerIsAttached_Grid_WindAnalytics)
        {
            Grid_ForecastAnalysis grid_windmodel = new Grid_ForecastAnalysis(sender, e, this);
            eventHandlerIsAttached_Grid_WindAnalytics = true;
        }
    }

    private void ButtonShowWindField_Click(object sender, RoutedEventArgs e)
    {
        grid_mapviever.ShowWindField(sender, e);
    }

    private void AddGrids()
    {
        this.grids = new List<Grid>();
        this.grids.Add(HomeScreen);
        this.grids.Add(Dashboard);
        this.grids.Add(UserLogin);
        this.grids.Add(MapViewer);
        this.grids.Add(WindAnalysis);
        this.grids.Add(ForecastAnalysis);
        this.grids.Add(SessionPlanner);
        this.grids.Add(Forum);
    }

    private void AddButtons()
    {
        this.buttons = new List<Button>();
        this.buttons.Add(buttonDashboard);
        this.buttons.Add(buttonMapViewer);
        this.buttons.Add(buttonWindAnalytics);
        this.buttons.Add(buttonForecastAnalysis);
        this.buttons.Add(buttonSessionPlanner);
        this.buttons.Add(buttonForum);
    }

    public void ChangeGrid(Grid act)
    {
        // Show user login if not logged in
        if (!UserSession.IsLoggedIn)
            act = UserLogin;

            foreach (Grid b in this.grids)
        {
            if (b != act)
                b.Visibility = System.Windows.Visibility.Hidden;
            else
                b.Visibility = System.Windows.Visibility.Visible;             
        }
    }

    private void ChangeColor(Button act)
    {
        foreach (Button b in this.buttons)
        {
            if (b != act)
                b.Background = new SolidColorBrush(Colors.WhiteSmoke);
            else
                b.Background = new SolidColorBrush(Colors.LightGoldenrodYellow);
        }
    }
}