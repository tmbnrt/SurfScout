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


namespace SurfScout;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private List<Button> buttons;
    private List<Grid> grids;
    public bool eventHandlerIsAttached_Grid_UserLogin = false;
    public bool eventHandlerIsAttached_Grid_MapViewer = false;
    public bool eventHandlerIsAttached_Grid_WindModel = false;

    public MainWindow()
    {
        InitializeComponent();

        // Maximized window
        WindowState = WindowState.Maximized;

        AddButtons();
        AddGrids();

        // Button interaction
        buttonUser.Click += ButtonUser_Click;
        buttonMapViewer.Click += ButtonMapViewer_Click;
        buttonWindModel.Click += ButtonWindModel_Click;
    }

    private void ButtonUser_Click(object sender, RoutedEventArgs e)
    {
        ChangeColor(buttonUser);
        ChangeGrid(UserLogin);

        if (!eventHandlerIsAttached_Grid_UserLogin)
        {
            Grid_UserLogin grid_userlogin = new Grid_UserLogin(sender, e, this);
            eventHandlerIsAttached_Grid_UserLogin = true;
        }
    }

    private void ButtonMapViewer_Click(object sender, RoutedEventArgs e)
    {
        ChangeColor(buttonMapViewer);
        ChangeGrid(MapViewer);

        if (!eventHandlerIsAttached_Grid_MapViewer)
        {
            Grid_MapViewer grid_mapviever = new Grid_MapViewer(sender, e, this);
            eventHandlerIsAttached_Grid_MapViewer = true;
        }
    }

    private void ButtonWindModel_Click(object sender, RoutedEventArgs e)
    {
        ChangeColor(buttonWindModel);
        ChangeGrid(WindModel);

        if (!eventHandlerIsAttached_Grid_WindModel)
        {
            Grid_WindModel grid_windmodel = new Grid_WindModel(sender, e, this);
            eventHandlerIsAttached_Grid_WindModel = true;
        }
    }

    private void AddGrids()
    {
        this.grids = new List<Grid>();
        this.grids.Add(HomeScreen);
        this.grids.Add(UserLogin);
        this.grids.Add(UserInfo);
        this.grids.Add(MapViewer);
        this.grids.Add(WindModel);
    }

    private void AddButtons()
    {
        this.buttons = new List<Button>();
        this.buttons.Add(buttonUser);
        this.buttons.Add(buttonMapViewer);
        this.buttons.Add(buttonWindModel);
    }

    public void ChangeGrid(Grid act)
    {
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