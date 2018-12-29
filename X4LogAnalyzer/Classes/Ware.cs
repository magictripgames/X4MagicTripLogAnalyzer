using X4LogAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X4LogAnalyzer
{
    public class Ware
    {
        private List<TradeOperation> _TradeOperationList = new List<TradeOperation>();
        //private static List<Ware> _WareList = MainWindow.GlobalWares;

        //public Ware(string ware)
        //{
        //    this.Name = ware;
        //    //_WareList.Add(this);
        //}

        public Ware()
        {
            //Default constructor for Deserialization
        }

        public string Name { get; set; }
        public string WareID { get; set; }
        public string TransportType { get; set; }
        public double MarketMinimumPrice { get; set; }

        public List<TradeOperation> GetTradeOperations()
        {
            return _TradeOperationList;
        }

        public double MarketAveragePrice { get; set; }
        public double MarketMaximumPrice { get; set; }
        public double Volume { get; set; }

        public double MaxAndMinPriceDifferencePerVolume
        {
            get
            {
                return (MarketMaximumPrice - MarketMinimumPrice) / Volume;
            }
        }

        public static Ware GetWare(string wareName)
        {
            Ware ware = MainWindow.GlobalWares.Where(x => x.Name.Equals(wareName)).FirstOrDefault();
            //if (ware == null)
            //{
            //    ware = new Ware(wareName);

            //}
            return ware;
        }

        internal void AddTradeOperation(TradeOperation tradeOperation)
        {
            TradeOperation tradeOp = _TradeOperationList.Where(x => x.Time == tradeOperation.Time).FirstOrDefault();
            if (tradeOp == null)
            {
                //tradeOp.ItemSold = this;
                tradeOperation.PartialSumByWare = _TradeOperationList.Sum(x => x.Money) + tradeOperation.Money;
                _TradeOperationList.Add(tradeOperation);

            }
        }
    }
}
