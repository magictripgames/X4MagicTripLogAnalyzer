using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X4LogAnalyzer
{
    public class Ship
    {
        private static List<Ship> _ShipList = new List<Ship>();
        private static List<Ship> _SoldToList = new List<Ship>();
        private List<TradeOperation> _TradeOperationList = new List<TradeOperation>();

        public Ship(string shipId)
        {
            this.ShipID = shipId;
            _ShipList.Add(this);
        }

        public string FullShipname
        {
            get
            {
                return string.Format("{0} ({1})", this.ShipName, this.ShipID);
            }
        }
        public string ShipID { get; set; }
        public string ShipName { get; set; }

        public static Ship GetShip(string shipId)
        {
            Ship selectedShip = _ShipList.Where(x => x.ShipID.Equals(shipId)).FirstOrDefault();
            if (selectedShip == null)
            {
                selectedShip = new Ship(shipId);
                _ShipList.Add(selectedShip);
                
            }
            return selectedShip;
        }

        public static Ship GetSoldTo(string shipId)
        {
            Ship selectedShip = _SoldToList.Where(x => x.ShipID.Equals(shipId)).FirstOrDefault();
            if (selectedShip == null)
            {
                selectedShip = new Ship(shipId);
                _SoldToList.Add(selectedShip);
            }
            return selectedShip;
        }


        public ChartValues<ObservablePoint> GetListOfTradeValues(int type)
        {
            ChartValues<ObservablePoint> valuesToReturn = new ChartValues<ObservablePoint>();

            double accumulatedMoney = 0;
            //double basePricePerItem = 0;
            double qtdItemsSold = 0;

            switch (type)
            {
                case 0:
                    accumulatedMoney = 0;
                    foreach (TradeOperation tradeOp in _TradeOperationList)
                    {
                        accumulatedMoney = accumulatedMoney + tradeOp.Money;
                        valuesToReturn.Add(new ObservablePoint(tradeOp.Time, accumulatedMoney));
                    }
                    break;
                case 1:
                    accumulatedMoney = 0;
                    foreach (TradeOperation tradeOp in _TradeOperationList)
                    {
                        qtdItemsSold = tradeOp.ItemSold.MarketMinimumPrice;
                        //basePricePerItem = tradeOp.Quantity;
                        accumulatedMoney = accumulatedMoney + tradeOp.EstimatedProfit;
                        valuesToReturn.Add(new ObservablePoint(tradeOp.Time, accumulatedMoney));
                    }
                    break;
                case 2:
                    foreach (TradeOperation tradeOp in _TradeOperationList)
                    {
                        valuesToReturn.Add(new ObservablePoint(tradeOp.Time, tradeOp.Money));
                    }
                    break;
                case 3:
                    foreach (TradeOperation tradeOp in _TradeOperationList)
                    {
                        valuesToReturn.Add(new ObservablePoint(tradeOp.Time, tradeOp.EstimatedProfit));
                    }
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }

            return valuesToReturn;
        }

        public void AddTradeOperation(TradeOperation tradeOperation)
        {
            TradeOperation tradeOp = _TradeOperationList.Where(x => x.Time == tradeOperation.Time).FirstOrDefault();
            if (tradeOp == null)
            {
                Console.WriteLine(string.Format("Ship: {0}; Item count: {1}, Time: {2}", this.FullShipname, _TradeOperationList.Count, tradeOperation.Time));
                _TradeOperationList.Add(tradeOperation);
                
            }
        }

        public IEnumerable<TradeOperation> GetListOfTradeOperations()
        {
            return _TradeOperationList;
        }
    }
}
