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
    public partial class ShipGraph : UserControl
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

        public ShipGraph()
        {
            InitializeComponent();
            MySeriesCollection = new SeriesCollection();
            //MySeriesCollectionRaw = new SeriesCollection();
            //MySeriesCollectionAccumulated = new SeriesCollection();
            //ChartValues<ObservablePoint> op = new ChartValues<ObservablePoint>();
            //op.Add(new ObservablePoint(10, 20));
            //op.Add(new ObservablePoint(10, 25));
            //MySeriesCollection.Add(new LineSeries
            //{
            //    Title = "Test",
            //    Values = (op)
            //});
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
                //tabablzControl.Style = style;
            }
            //this.DataContext = MySeriesCollection;
            //DataContext = this;
        }

        //private bool IsSelectorChecked = false;
        private bool ShowIndividualPrices = false;
        private bool ShowWithReducedEstimatedWareCost = false;

        public static SeriesCollection MySeriesCollection { get; set; }
        //public static SeriesCollection MySeriesCollectionRawWithFullPrice { get; set; }
        //public static SeriesCollection MySeriesCollectionRawWithReducedPrices { get; set; }
        //public static SeriesCollection MySeriesCollectionAccumulated { get; set; }
        //public string[] Labels { get; set; }
        //public Func<double, string> YFormatter { get; set; }
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

        //private List<Ship> SelectedShips = new List<Ship>();
        //private List<Ship> FullShipList = new List<Ship>();
        //private ObservableCollection<TradeOperation> ShipList = new ObservableCollection<TradeOperation>();
        //private ObservableCollection<TradeOperation> FullList = new ObservableCollection<TradeOperation>();
        //private ObservableCollection<TradeOperation> FilteredList = new ObservableCollection<TradeOperation>();

        private void ShipList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //FullList.Clear();
            //FilteredList.Clear();

            //Ship selectedShip = SelectedShips.Where(x => x.FullShipname.Equals(((X4LogAnalyzer.TradeOperation)((object[])e.AddedItems)[0]).OurShip.FullShipname)).FirstOrDefault();
            //if (selectedShip != null)
            //{
            //    SelectedShips.Add(selectedShip);
            //}

            //if (FullShipList.Count > 0)
            //{
            //    Ship selectedShip = SelectedShips.Where(x => x.FullShipname.Equals(((X4LogAnalyzer.TradeOperation)((object[])e.AddedItems)[0]).OurShip.FullShipname)).FirstOrDefault();
            //    if (selectedShip != null)
            //    {
            //        SelectedShips.Add(selectedShip);
            //    }

            //    RefreshGraph();
            //    //List<TradeOperation> filteredList = FullList.Where(i => i.OurShip.ShipID.Equals(((X4LogAnalyzer.TradeOperation)((object[])e.AddedItems)[0]).OurShip.ShipID)).OrderBy(x => x.Time).ToList();
            //    //string shipName = filteredList[filteredList.Count() - 1].OurShip.ShipName;
            //    //double accumulatedMoney = 0;
            //    //ChartValues<ObservablePoint> moneyValuesRaw = new ChartValues<ObservablePoint>();
            //    //ChartValues<ObservablePoint> moneyValuesAccumulated = new ChartValues<ObservablePoint>();

            //    //if (MySeriesCollection.Where(x => x.Title.Equals(shipName)).Count() != 0)
            //    //{
            //    //    //Don't add the ship again if it's already added
            //    //    return;
            //    //}

            //    //foreach (TradeOperation tradeOp in filteredList)
            //    //{
            //    //    accumulatedMoney = accumulatedMoney + tradeOp.Money;
            //    //    moneyValuesAccumulated.Add(new ObservablePoint(tradeOp.Time, accumulatedMoney));
            //    //    moneyValuesRaw.Add(new ObservablePoint(tradeOp.Time, tradeOp.Money));

            //    //    //FilteredList.Add(tradeOp);
            //    //}

            //    //MySeriesCollectionRaw.Add(new LineSeries
            //    //{
            //    //    Title = filteredList[filteredList.Count()-1].OurShip.ShipName,
            //    //    Values = moneyValuesRaw
            //    // });
            //    //MySeriesCollectionAccumulated.Add(new LineSeries
            //    //{
            //    //    Title = filteredList[filteredList.Count() - 1].OurShip.ShipName,
            //    //    Values = moneyValuesAccumulated
            //    //});

            //    //if (IsSelectorChecked)
            //    //{
            //    //    MySeriesCollection = MySeriesCollectionRaw;
            //    //}
            //    //else
            //    //{
            //    //    MySeriesCollection = MySeriesCollectionAccumulated;
            //    //}

            //    RefreshGraph();

            //    //ChartShip.DataContext = MySeriesCollection;
            //    //this.DataContext = MySeriesCollection;
            //    //DataContext = this;

            //}
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ShowIndividualPrices = true;
            //RefreshGraph();

            //IsSelectorChecked = true;
        }

        private void AddItemToTheGraph()
        {

            //Check to toggle individual sales vs sum of sales
            //MySeriesCollection = MySeriesCollectionRaw;
            //MySeriesCollection = MySeriesCollectionAccumulated;
            //MySeriesCollection.Clear();
            //foreach (Ship ship in )
            //{

            //}

            //this.DataContext = MySeriesCollection;
            //DataContext = this;
            //throw new NotImplementedException();
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowIndividualPrices = false;
            //Check to toggle individual sales vs sum of sales
            //ShowIndividualPrices = false;
            //RefreshGraph();
        }

        private void ToggleButton_Checked_1(object sender, RoutedEventArgs e)
        {
            //Check to toggle to reduce the approximaded cost of wares
            ShowWithReducedEstimatedWareCost = true;
            //RefreshGraph();
        }

        private void ToggleButton_Unchecked_1(object sender, RoutedEventArgs e)
        {
            //Check to toggle to reduce the approximaded cost of wares
            ShowWithReducedEstimatedWareCost = false;
            //RefreshGraph();
        }

        private void ShipList_MouseClick(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Click - MouseDown");
        }

        private void ShipList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if ("Ship".Equals(((object[])e.AddedItems)[0].GetType().Name))
            //{
            //    //This is required when the inner textlist is been selected
            //    return;
            //}
            Ship ship = ((X4LogAnalyzer.Ship)((System.Windows.Controls.Primitives.Selector)e.Source).SelectedItem);
            AddPenToGraph(ship);
            this.DataContext = MySeriesCollection;
            DataContext = this;

            //Console.WriteLine("DoubleClick");
            //if (FullList.Count > 0)
            //{
            //    string shipID = ((X4LogAnalyzer.TradeOperation)((System.Windows.Controls.Primitives.Selector)e.Source).SelectedItem).OurShip.ShipName;
            //    var item = MySeriesCollection.Where(x => x.Title.Equals(shipID)).FirstOrDefault();
            //    if (item != null)
            //    {
            //        MySeriesCollection.Remove(item);
            //    }
            //    item = MySeriesCollectionRaw.Where(x => x.Title.Equals(shipID)).FirstOrDefault();
            //    if (item != null)
            //    {
            //        try
            //        {
            //            //Don't know why this keep failing
            //            MySeriesCollectionRaw.Remove(item);
            //        }
            //        catch (Exception err)
            //        {
            //            Console.WriteLine(err.Message);
            //        }

            //    }
            //    item = MySeriesCollectionAccumulated.Where(x => x.Title.Equals(shipID)).FirstOrDefault();
            //    if (item != null)
            //    {
            //        MySeriesCollectionAccumulated.Remove(item);
            //    }

            //    //MySeriesCollectionRaw.Where(x => x.Title.Equals(shipID)).FirstOrDefault().Erase(true);
            //    //MySeriesCollectionAccumulated.Where(x => x.Title.Equals(shipID)).FirstOrDefault().Erase(true);
            //    //List<TradeOperation> filteredList = FullList.Where(i => i.ShipId.Equals(((X4LogAnalyzer.TradeOperation)((object[])e.AddedItems)[0]).ShipId)).OrderBy(x => x.Time).ToList();
            //    //string shipName = filteredList[filteredList.Count() - 1].ShipName;
            //    //double accumulatedMoney = 0;
            //    //ChartValues<ObservablePoint> moneyValuesRaw = new ChartValues<ObservablePoint>();
            //    //ChartValues<ObservablePoint> moneyValuesAccumulated = new ChartValues<ObservablePoint>();

            //    //if (MySeriesCollection.Where(x => x.Title.Equals(shipName)).Count() != 0)
            //    //{
            //    //    //Don't add the ship again if it's already added
            //    //    return;
            //    //}

            //    //foreach (TradeOperation tradeOp in filteredList)
            //    //{
            //    //    accumulatedMoney = accumulatedMoney + tradeOp.Money;
            //    //    moneyValuesAccumulated.Add(new ObservablePoint(tradeOp.Time, accumulatedMoney));
            //    //    moneyValuesRaw.Add(new ObservablePoint(tradeOp.Time, tradeOp.Money));

            //    //    //FilteredList.Add(tradeOp);
            //    //}

            //    //MySeriesCollectionRaw.Add(new LineSeries
            //    //{
            //    //    Title = filteredList[filteredList.Count() - 1].ShipName,
            //    //    Values = moneyValuesRaw
            //    //});
            //    //MySeriesCollectionAccumulated.Add(new LineSeries
            //    //{
            //    //    Title = filteredList[filteredList.Count() - 1].ShipName,
            //    //    Values = moneyValuesAccumulated
            //    //});

            //    //if (IsSelectorChecked)
            //    //{
            //    //    MySeriesCollection = MySeriesCollectionRaw;
            //    //}
            //    //else
            //    //{
            //    //    MySeriesCollection = MySeriesCollectionAccumulated;
            //    //}

            //    //ChartShip.DataContext = MySeriesCollection;
            //    this.DataContext = MySeriesCollection;
            //    DataContext = this;

            //}
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
            //if (PensAddedToTheGraph.Count == 0)
            //{
            //    PensAddedToTheGraph.Add(new PenAdded(ship, penToAdd));
            //    MySeriesCollection.Add(new LineSeries
            //    {
            //        Title = ship.FullShipname,
            //        Values = ship.GetListOfTradeValues(penToAdd)
            //    });
            //}
            //else
            //{
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
            //}
            
            //throw new NotImplementedException();
        }
    }
}
