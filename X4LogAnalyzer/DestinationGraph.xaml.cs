using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using X4LogAnalyzer;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.Generic;
using LiveCharts.Defaults;
using System.Windows.Input;

namespace X4LogAnalyzer
{
    /// <summary>
    /// Interaction logic for MahApps.xaml
    /// </summary>
    public partial class DestinationGraph : UserControl
    {
        private class PenAdded
        {
            public Ship ShipAdded { get; set; }
            public int TypeOfPen { get; set; }
            public static int ACCUMULATED_UNCHANGEDPRICES = 0;
            public static int ACCUMULATED_REDUCEDPRICES = 1;
            public static int INDIVIDUALPRICE_UNCHANGEDPRICES = 2;
            public static int INDIVIDUALPRICE_REDUCEDPRICES = 3;

            public PenAdded(Ship ship, int type)
            {
                this.ShipAdded = ship;
                this.TypeOfPen = type;
            }
            //types of pen:
            //0 = Raw and Unchanged Prices
            //1 = Raw and Reduced Ware Prices
            //2 = Item price and Unchanged Prices
            //3 = Item price and Reduced Ware Prices
        }

        private List<PenAdded> PensAddedToTheGraph = new List<PenAdded>();

        public DestinationGraph()
        {
            InitializeComponent();
            MySeriesCollection = new SeriesCollection();
            DataContext = this;
        }

        public ICommand ToggleStyleCommand { get; } = new AnotherCommandImplementation(o => TobleBetweenRawAndAccumulatedValue((bool)o));

        private static void TobleBetweenRawAndAccumulatedValue(bool o)
        {
            //True is accumulated, false is raw
            //throw new NotImplementedException();
            foreach (var tabablzControl in Dragablz.TabablzControl.GetLoadedInstances())
            {
                Console.WriteLine("ToggleStyleCommand");
            }
        }

        private bool ShowIndividualPrices = false;
        private bool ShowWithReducedEstimatedWareCost = false;

        public static SeriesCollection MySeriesCollection { get; set; }
        private double _axisMax;
        private double _axisMin;

        public double AxisMax
        {
            get { return _axisMax; }
            set
            {
                _axisMax = value;
                OnPropertyChanged("AxisMax");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null) // if subrscribed to event
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public double AxisMin
        {
            get { return _axisMin; }
            set
            {
                _axisMin = value;
                OnPropertyChanged("AxisMin");
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((System.Windows.UIElement)sender).IsVisible)
            {
                FillInShipList();
                MySeriesCollection.Clear();
                PensAddedToTheGraph.Clear();
                this.DataContext = MySeriesCollection;
                DataContext = this;
            }
        }

        private void FillInShipList()
        {
            shipList.ItemsSource = MainWindow.ShipsWithTradeOperations.OrderBy(i => i.FullShipname);
            DataContext = this;
        }


        private void ShipList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ShowIndividualPrices = true;
        }

        private void AddItemToTheGraph()
        {

        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowIndividualPrices = false;
        }

        private void ToggleButton_Checked_1(object sender, RoutedEventArgs e)
        {
            //Check to toggle to reduce the approximaded cost of wares
            ShowWithReducedEstimatedWareCost = true;
        }

        private void ToggleButton_Unchecked_1(object sender, RoutedEventArgs e)
        {
            //Check to toggle to reduce the approximaded cost of wares
            ShowWithReducedEstimatedWareCost = false;
        }

        private void ShipList_MouseClick(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Click - MouseDown");
        }

        private void ShipList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Ship ship = ((X4LogAnalyzer.Ship)((System.Windows.Controls.Primitives.Selector)e.Source).SelectedItem);
            AddPenToGraph(ship);
            this.DataContext = MySeriesCollection;
            DataContext = this;

        }

        private int GetPenToAdd()
        {
            int pen = 0;
            if (!ShowIndividualPrices && !ShowWithReducedEstimatedWareCost)
            {
                pen = PenAdded.ACCUMULATED_UNCHANGEDPRICES;
            }
            if (!ShowIndividualPrices && ShowWithReducedEstimatedWareCost)
            {
                pen = PenAdded.ACCUMULATED_REDUCEDPRICES;
            }
            if (ShowIndividualPrices && !ShowWithReducedEstimatedWareCost)
            {
                pen = PenAdded.INDIVIDUALPRICE_UNCHANGEDPRICES;
            }
            if (ShowIndividualPrices && ShowWithReducedEstimatedWareCost)
            {
                pen = PenAdded.INDIVIDUALPRICE_REDUCEDPRICES;
            }
            return pen;
        }

        private void AddPenToGraph(Ship ship)
        {
            int penToAdd = GetPenToAdd();
            PenAdded pen = PensAddedToTheGraph.Where(x => x.ShipAdded.FullShipname.Equals(ship.FullShipname) && x.TypeOfPen == penToAdd).FirstOrDefault();
            if (pen == null)
            {
                PensAddedToTheGraph.Add(new PenAdded(ship, penToAdd));
                MySeriesCollection.Add(new LineSeries
                {
                    Title = ship.FullShipname,
                    Values = ship.GetListOfTradeValues(penToAdd)
                });
            }
        }
    }
}
