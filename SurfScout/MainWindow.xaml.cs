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


namespace SurfScout;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private List<Button> buttons;
    private List<Grid> grids;
    public bool eventHandlerIsAttached_Grid_MapViewer = false;
    public bool eventHandlerIsAttached_Grid_WindModel = false;

    public MainWindow()
    {
        InitializeComponent();

        AddButtons();
        AddGrids();

        // Button interaction
        buttonMapViewer.Click += buttonMapViewer_Click;
        buttonWindModel.Click += buttonWindModel_Click;
    }

    private void buttonMapViewer_Click(object sender, RoutedEventArgs e)
    {
        ChangeColor(buttonMapViewer);
        ChangeGrid(MapViewer);

        if (!eventHandlerIsAttached_Grid_MapViewer)
        {
            Grid_MapViewer grid_mapviever = new Grid_MapViewer(sender, e, this);
            eventHandlerIsAttached_Grid_MapViewer = true;
        }
    }

    private void buttonWindModel_Click(object sender, RoutedEventArgs e)
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
        this.grids.Add(MapViewer);
        this.grids.Add(WindModel);
    }

    private void AddButtons()
    {
        this.buttons = new List<Button>();
        this.buttons.Add(buttonMapViewer);
        this.buttons.Add(buttonWindModel);
    }

    private void ChangeGrid(Grid act)
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