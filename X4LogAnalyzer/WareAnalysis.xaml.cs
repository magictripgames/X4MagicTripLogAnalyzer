using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace X4LogAnalyzer
{
    /// <summary>
    /// Interaction logic for WareAnalysis.xaml
    /// </summary>
    public partial class WareAnalysis : UserControl
    {
        private class WaresSummary
        {
            public WaresSummary()
            {

            }
            public Ware Ware { get; set; }
            public double QuantitySold { get; set; }
            public double TotalValueSold { get; set; }
            public double TotalProfit { get; set; }
        }
        private class ShipsSummary
        {
            public ShipsSummary()
            {

            }
            public Ship Ship { get; set; }
            public double QuantitySold { get; set; }
            public double TotalValueSold { get; set; }
            public double TotalProfit { get; set; }
            public Ware Ware { get; set; }
        }

        private List<WaresSummary> WaresSummaries = new List<WaresSummary>();

        public WareAnalysis()
        {
            InitializeComponent();
            SeriesCollection = new SeriesCollection();
            //SeriesCollectionTotalSales = new SeriesCollection();
        }

        //private SeriesCollection SeriesCollection = new SeriesCollection();
        public SeriesCollection SeriesCollection { get; set; }
        //public SeriesCollection SeriesCollectionTotalSales { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((System.Windows.UIElement)sender).IsVisible)
            {
                SeriesCollection.Clear();
                WaresSummaries.Clear();
                FillInWaresSummaryList();
                this.DataContext = SeriesCollection;
                DataContext = this;
            }

            
        }

        private void FillInWaresSummaryList()
        {
            
            foreach (Ware ware in MainWindow.GlobalWares)
            {
                WaresSummary waresSummary = new WaresSummary();
                waresSummary.Ware = ware;
                waresSummary.TotalProfit = ware.GetTradeOperations().Sum(x => x.EstimatedProfit);
                waresSummary.TotalValueSold = ware.GetTradeOperations().Sum(x => x.Money);
                waresSummary.QuantitySold = ware.GetTradeOperations().Sum(x => x.Quantity);
                if (waresSummary.TotalProfit > 0)
                {
                    WaresSummaries.Add(waresSummary);
                }
            }

            foreach (WaresSummary waresSummary in WaresSummaries.OrderByDescending(x => x.TotalProfit))
            {
                ColumnSeries column = new ColumnSeries { Title = waresSummary.Ware.Name, Values = new ChartValues<double> { waresSummary.TotalProfit } };
                SeriesCollection.Add(column);
            }

            

            //histogram.DataTooltip.SelectionMode = TooltipSelectionMode.OnlySender;
            //SeriesCollection = new SeriesCollection
            //{
            //    new ColumnSeries
            //    {
            //        Title = "2015",
            //        Values = new ChartValues<double> { 10, 50, 39, 50 }
            //    }
            //};

            ////adding series will update and animate the chart automatically
            //SeriesCollection.Add(new ColumnSeries
            //{
            //    Title = "2016",
            //    Values = new ChartValues<double> { 11, 56, 42 }
            //});

            ////also adding values updates and animates the chart automatically
            //SeriesCollection[1].Values.Add(48d);

            //Labels = new[] { "Maria", "Susan", "Charles", "Frida" };
            //Formatter = value => value.ToString("N");

            DataContext = this;
        }

        private void ShowEstimatedProfitRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (SeriesCollection == null)
            {
                SeriesCollection = new SeriesCollection();
            }
            SeriesCollection.Clear();
            foreach (WaresSummary waresSummary in WaresSummaries.OrderByDescending(x => x.TotalProfit))
            {
                ColumnSeries column = new ColumnSeries { Title = waresSummary.Ware.Name, Values = new ChartValues<double> { waresSummary.TotalProfit } };
                SeriesCollection.Add(column);
            }
        }

        private void ShowFullMoneyEarnedRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (SeriesCollection == null)
            {
                SeriesCollection = new SeriesCollection();
            }
            SeriesCollection.Clear();
            foreach (WaresSummary waresSummary in WaresSummaries.OrderByDescending(x => x.TotalValueSold))
            {
                ColumnSeries column = new ColumnSeries { Title = waresSummary.Ware.Name, Values = new ChartValues<double> { waresSummary.TotalValueSold } };
                SeriesCollection.Add(column);
            }
        }

        private void ShowTotalItemsRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (SeriesCollection == null)
            {
                SeriesCollection = new SeriesCollection();
            }
            SeriesCollection.Clear();
            foreach (WaresSummary waresSummary in WaresSummaries.OrderByDescending(x => x.QuantitySold))
            {
                ColumnSeries column = new ColumnSeries { Title = waresSummary.Ware.Name, Values = new ChartValues<double> { waresSummary.QuantitySold } };
                SeriesCollection.Add(column);
            }
        }

        private void Histogram_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Double Click");
        }

        private void Histogram_DataClick(object sender, ChartPoint chartPoint)
        {
            //ShowEstimatedProfitRadio.IsChecked = false;
            //ShowFullMoneyEarnedRadio.IsChecked = false;
            //ShowTotalItemsRadio.IsChecked = false;
            Ware ware = null;
            try
            {
                ware = WaresSummaries.Where(x => x.Ware.Name.Equals(((LiveCharts.Wpf.Series)chartPoint.SeriesView).Title)).FirstOrDefault().Ware;
                SeriesCollection.Clear();
            }
            catch (Exception)
            {
                //TODO: Bad programming, improve this and probably show items sold by ship
                return;
                //throw;
            }
            

            List<ShipsSummary> shipsSummaries = new List<ShipsSummary>();
            foreach (Ship ship in MainWindow.ShipsWithTradeOperations)
            {
                List<TradeOperation> tradeOperations = ship.GetListOfTradeOperations().Where(x=>x.ItemSold.Name.Equals(ware.Name)).ToList();
                ShipsSummary shipSummary = new ShipsSummary();
                shipSummary.Ship = ship;
                shipSummary.Ware = ware;
                shipSummary.TotalProfit = tradeOperations.Sum(x => x.EstimatedProfit);
                shipSummary.TotalValueSold = tradeOperations.Sum(x => x.Money);
                shipSummary.QuantitySold = tradeOperations.Sum(x => x.Quantity);
                shipsSummaries.Add(shipSummary);
            }

            foreach (ShipsSummary shipsummary in shipsSummaries.OrderByDescending(x => x.QuantitySold))
            {
                if (shipsummary.QuantitySold > 0)
                {
                    ColumnSeries column = null;
                    if (ShowEstimatedProfitRadio.IsChecked.Value)
                    {
                        column = new ColumnSeries { Title = shipsummary.Ship.FullShipname, Values = new ChartValues<double> { shipsummary.TotalProfit } };
                    }
                    if (ShowFullMoneyEarnedRadio.IsChecked.Value)
                    {
                        column = new ColumnSeries { Title = shipsummary.Ship.FullShipname, Values = new ChartValues<double> { shipsummary.TotalValueSold } };
                    }
                    if (ShowTotalItemsRadio.IsChecked.Value)
                    {
                        column = new ColumnSeries { Title = shipsummary.Ship.FullShipname, Values = new ChartValues<double> { shipsummary.QuantitySold } };
                    }
                    
                    SeriesCollection.Add(column);
                }
                
            }
            //Console.WriteLine("Double Click");
        }
    }
}
