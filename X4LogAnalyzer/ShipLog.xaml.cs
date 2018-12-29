using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using X4LogAnalyzer;

namespace X4LogAnalyzer
{
    /// <summary>
    /// Interaction logic for MahApps.xaml
    /// </summary>
    public partial class ShipLog : UserControl
    {
        public interface ICustomSorter : IComparer
        {
            ListSortDirection SortDirection { get; set; }
        }

        public class CustomSortBehaviour
        {
            public static readonly DependencyProperty CustomSorterProperty =
                DependencyProperty.RegisterAttached("CustomSorter", typeof(ICustomSorter), typeof(CustomSortBehaviour));

            public static ICustomSorter GetCustomSorter(DataGridColumn gridColumn)
            {
                return (ICustomSorter)gridColumn.GetValue(CustomSorterProperty);
            }

            public static void SetCustomSorter(DataGridColumn gridColumn, ICustomSorter value)
            {
                gridColumn.SetValue(CustomSorterProperty, value);
            }

            public static readonly DependencyProperty AllowCustomSortProperty =
                DependencyProperty.RegisterAttached("AllowCustomSort", typeof(bool),
                typeof(CustomSortBehaviour), new UIPropertyMetadata(false, OnAllowCustomSortChanged));

            public static bool GetAllowCustomSort(DataGrid grid)
            {
                return (bool)grid.GetValue(AllowCustomSortProperty);
            }

            public static void SetAllowCustomSort(DataGrid grid, bool value)
            {
                grid.SetValue(AllowCustomSortProperty, value);
            }

            private static void OnAllowCustomSortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                var existing = d as DataGrid;
                if (existing == null) return;

                var oldAllow = (bool)e.OldValue;
                var newAllow = (bool)e.NewValue;

                if (!oldAllow && newAllow)
                {
                    existing.Sorting += HandleCustomSorting;
                }
                else
                {
                    existing.Sorting -= HandleCustomSorting;
                }
            }

            private static void HandleCustomSorting(object sender, DataGridSortingEventArgs e)
            {
                var dataGrid = sender as DataGrid;
                if (dataGrid == null || !GetAllowCustomSort(dataGrid)) return;

                var listColView = dataGrid.ItemsSource as ListCollectionView;
                if (listColView == null)
                    throw new Exception("The DataGrid's ItemsSource property must be of type, ListCollectionView");

                // Sanity check
                var sorter = GetCustomSorter(e.Column);
                if (sorter == null) return;

                // The guts.
                e.Handled = true;

                var direction = (e.Column.SortDirection != ListSortDirection.Ascending)
                                    ? ListSortDirection.Ascending
                                    : ListSortDirection.Descending;

                e.Column.SortDirection = sorter.SortDirection = direction;
                listColView.CustomSort = sorter;
            }
        }

        private ObservableCollection<TradeOperation> TradeOperations = new ObservableCollection<TradeOperation>();
        //private ObservableCollection<TradeOperation> FullList = new ObservableCollection<TradeOperation>();
        //private ObservableCollection<TradeOperation> FilteredList = new ObservableCollection<TradeOperation>();
        public class Total
        {
            private double _TotalMoneyCollected;
            private double _TimeInService;
            public string TotalItemsTraded { get; set; }
            public string TotalMoneyCollected
            {
                get
                {
                    return _TotalMoneyCollected.ToString("C0");
                }
                set { this._TotalMoneyCollected = double.Parse(value); }
            }
            public string TimeInService
            {
                get
                {
                    TimeSpan span = TimeSpan.FromSeconds(_TimeInService);
                    return string.Format("{0}:{1}:{2}", (int)span.TotalHours, span.Minutes.ToString("00"), span.Seconds.ToString("00"));
                    //return _TotalMoneyCollected.ToString("C0");
                }
                set { this._TimeInService = double.Parse(value); }
            }
        }

        public ShipLog()
        {
            InitializeComponent();
        }

        public void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            
        }

        private void FillInShipList()
        {
            shipList.ItemsSource = MainWindow.ShipsWithTradeOperations.OrderBy(i => i.FullShipname);
            DataContext = this;
        }

        private void UserControl_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (((System.Windows.UIElement)sender).IsVisible)
            {
                if (MainWindow.GlobalTradeOperations.Count() == 0)
                {
                    return;
                }
                //ShipList.Clear();
                //FilteredList.Clear();
                //int QtdTradedValue = 0;
                //int ValueTotalTradedValue = 0;
                Total total = new Total();
                {
                    total.TotalItemsTraded = MainWindow.GlobalTradeOperations.Sum(x => x.Quantity).ToString();
                    total.TotalMoneyCollected = MainWindow.GlobalTradeOperations.Sum(x => x.Money).ToString();
                    double minTime = MainWindow.GlobalTradeOperations.Min(x => x.Time);
                    double maxTime = MainWindow.GlobalTradeOperations.Max(x => x.Time);
                    total.TimeInService = (maxTime - minTime).ToString();
                }

                FillInShipList();
                //IQueryable<TradeOperation> queriableList = MainWindow.GlobalTradeOperations.AsQueryable<TradeOperation>();
                //foreach (TradeOperation tradeOp in MainWindow.GlobalTradeOperations.OrderBy(i => i.Time))
                //{
                //    QtdTradedValue = QtdTradedValue + tradeOp.Quantity;
                //    ValueTotalTradedValue = ValueTotalTradedValue + tradeOp.Money;
                //    total.TotalMoneyCollected = ValueTotalTradedValue.ToString();
                //    total.TotalItemsTraded = QtdTradedValue.ToString();
                //    FilteredList.Add(tradeOp);
                //    FullList.Add(tradeOp);
                //}
                //foreach (Ship ship in MainWindow.ShipsWithTradeOperations)
                //{
                //    ShipList.Add(tradeOp);
                //}
                //if (ShipList.Count == 0)
                //{
                //    ShipList.Add(new TradeOperation(0, "Please load the XML", "", "", 0, "", "", "", "", 0));
                //}
                //shipList.ItemsSource = ShipList.OrderBy(i => i.OurShip.ShipName);
                //TradeOpGrid.ItemsSource = FilteredList;
                //double minTime = 0;
                //double maxTime = 0;
                //if (FilteredList.Count > 0)
                //{
                //    minTime = FilteredList.ToList().Min(x => x.Time);
                //    maxTime = FilteredList.ToList().Max(x => x.Time);
                //    total.TimeInService = (maxTime - minTime).ToString();
                //}
                //else
                //{
                //    total.TimeInService = "0";
                //}
                TradeOpGrid.ItemsSource = TradeOperations.OrderBy(x => x.Time);
                this.DataContext = total;
                Console.WriteLine("Tab");
            }
            
        }

        private void ShipList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (MainWindow.GlobalTradeOperations.Count == 0)
            //{
            //    FullList.Clear();
            //    return;
            //}
            TradeOperations.Clear();
            Ship ship = ((X4LogAnalyzer.Ship)((System.Windows.Controls.Primitives.Selector)e.Source).SelectedItem);
            foreach (TradeOperation tradeOp in ship.GetListOfTradeOperations().OrderBy(x => x.Time))
            {
                TradeOperations.Add(tradeOp);
            }
            //AddPenToGraph(ship);
            //this.DataContext = TradeOperations;
            //DataContext = this;
            double minTime = 0;
            double maxTime = 0;
            //FilteredList.Clear();
            Total total = new Total();
            {
                total.TotalItemsTraded = ship.GetListOfTradeOperations().Sum(x => x.Quantity).ToString();
                total.TotalMoneyCollected = ship.GetListOfTradeOperations().Sum(x => x.Money).ToString();
                minTime = ship.GetListOfTradeOperations().Min(x => x.Time);
                maxTime = ship.GetListOfTradeOperations().Max(x => x.Time);
                total.TimeInService = (maxTime - minTime).ToString();
            }

            //int QtdTradedValue = 0;
            //int ValueTotalTradedValue = 0;

            //if (FullList.Count > 0)
            //{
            //    foreach (TradeOperation tradeOp in FullList.Where(i => i.OurShip.ShipID.Equals(((X4LogAnalyzer.TradeOperation)((object[])e.AddedItems)[0]).OurShip.ShipID)))
            //    {
            //        QtdTradedValue = QtdTradedValue + tradeOp.Quantity;
            //        ValueTotalTradedValue = ValueTotalTradedValue + tradeOp.Money;
            //        total.TotalMoneyCollected = ValueTotalTradedValue.ToString();
            //        total.TotalItemsTraded = QtdTradedValue.ToString();
            //        FilteredList.Add(tradeOp);
            //    }
            //    minTime = FilteredList.ToList().Min(x => x.Time);
            //    maxTime = FilteredList.ToList().Max(x => x.Time);
            //    total.TimeInService = (maxTime - minTime).ToString();
            //}
            //else
            //{
            //    total.TimeInService = "0";
            //}

            TradeOpGrid.ItemsSource = TradeOperations.OrderBy(x => x.Time);
            this.DataContext = total;

        }
    }
}
