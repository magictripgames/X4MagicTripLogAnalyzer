using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace X4LogAnalyzer
{
    public class TradeOperation
    {
        public double Time { get; set; }
        public string TimeRounded { get { return Time.ToString("0.00"); } }
        public Ship OurShip { get; set; }
        public string FullLogEntry { get; set; }
        public Ware ItemSold { get; set; }
        public int Quantity { get; set; }
        public Ship SoldTo { get; set; }
        public string Faction { get; set; }

        public double InventorySpaceUsed
        {
            get
            {
                return Quantity * ItemSold.Volume;
            }
        }
        public int Money { get; set; }
        public string MoneyFormated { get { return Money.ToString("C0"); } }
        public string PricePerItemFormated { get { return (Money / Quantity).ToString("C0"); } }
        public double PricePerItem {get { return (Money / Quantity); } }
        public double EstimatedProfit
        {
            get
            {
                double estimatedSoldPrice = 0;
                //I am disconsidering Ware price when it's a primary product (stored on Solid or Liquid storage)
                if ("Container".Equals(ItemSold.TransportType))
                {
                    estimatedSoldPrice = ItemSold.MarketMinimumPrice;
                }
                return (PricePerItem - estimatedSoldPrice) * Quantity;
            }
        }
        public string EstimatedProfitFormated { get { return EstimatedProfit.ToString("C0"); } }

        public string TimeFormated {
            get
            {
                TimeSpan span = TimeSpan.FromSeconds(Time);
                return string.Format("{0}:{1}:{2}", (int)span.TotalHours, span.Minutes.ToString("00"), span.Seconds.ToString("00"));
            }
        }
        public double PartialSumByShip { get; set; }
        public string PartialSumByShipFormated { get { return PartialSumByShip.ToString("C0"); } }
        public double PartialSumByWare { get; set; }
        public string PartialSumByWareFormated { get { return PartialSumByWare.ToString("C0"); } }

        public TradeOperation(double time, string shipId, string fullLogEntry, string shipName, int quantity, string product, string soldToId, string soldToName, string faction, int money)
        {
            if ("Please load the XML".Equals(shipId))
            {
                return;
            }
            this.Time = time;
            Ship ourShip = Ship.GetShip(shipId);
            //this is to ensure the last name of the ship sticks
            OurShip.ShipName = shipName;
            
            this.OurShip = ourShip;
            this.FullLogEntry = fullLogEntry;
            this.Quantity = quantity;

            Ware itemSold = Ware.GetWare(product);
            this.ItemSold = itemSold;

            this.SoldTo = Ship.GetSoldTo(soldToId);
            this.SoldTo.ShipName = soldToName;
            this.Faction = faction;
            this.Money = money;
            ourShip.AddTradeOperation(this);
            PartialSumByShip = OurShip.GetListOfTradeOperations().Sum(x => x.Money);
            PartialSumByWare = 0; 

            
            itemSold.AddTradeOperation(this);
        }

        public TradeOperation()
        {
            //This is required for the desserialization process
        }

        public TradeOperation(double time)
        {
            this.Time = time;
        }

        public void WriteToLog()
        {
            Console.WriteLine("\t{");
            Console.WriteLine(string.Format("\t   {0} : {1}", "Time", this.Time));
            Console.WriteLine(string.Format("\t   {0} : {1}", "ShipId", this.OurShip.ShipID));
            Console.WriteLine(string.Format("\t   {0} : {1}", "ShipName", this.OurShip.ShipName));
            Console.WriteLine(string.Format("\t   {0} : {1}", "Quantity", this.Quantity));
            Console.WriteLine(string.Format("\t   {0} : {1}", "Product", this.ItemSold.Name));
            Console.WriteLine(string.Format("\t   {0} : {1}", "SoldToID", this.SoldTo.ShipID));
            Console.WriteLine(string.Format("\t   {0} : {1}", "SoldToName", this.SoldTo.ShipName));
            Console.WriteLine(string.Format("\t   {0} : {1}", "Faction", this.Faction));
            Console.WriteLine(string.Format("\t   {0} : {1}", "Money", this.Money));
            Console.WriteLine(string.Format("\t   {0} : {1}", "FullLogEntry", this.FullLogEntry));
            Console.WriteLine("\t}");
        }

        public void ParseTextEntry(XmlReader logEntry)
        {
            this.FullLogEntry = logEntry.Value;
            this.OurShip = Ship.GetShip(getShipID(logEntry.Value));
            this.OurShip.ShipName = getShipName(logEntry.Value, this.OurShip.ShipID);
            this.Quantity = getQtdSold(logEntry.Value);
            this.ItemSold = Ware.GetWare(getProduct(logEntry.Value, this.Quantity));
            this.SoldTo = Ship.GetSoldTo(getDestinationID(logEntry.Value));
            this.SoldTo.ShipName = getSoldToName(logEntry.Value, this.SoldTo.ShipID);
            
        }

        private static string getDestinationID(string logEntry)
        {
            int position;
            string configEntry = MainWindow.Configurations.Where(x => x.Key.Equals("InTranslation")).FirstOrDefault().Value;
            configEntry = " " + configEntry.Trim() + " ";
            if (logEntry.Contains(configEntry))
            {
                position = logEntry.IndexOf(configEntry, 0);
                return logEntry.Substring(position - 7, 7);
            }
            else
            {
                return "";
            }
        }

        private static string getSoldToName(string logEntry, string soldToId)
        {
            string configEntryIn = MainWindow.Configurations.Where(x => x.Key.Equals("InTranslation")).FirstOrDefault().Value;
            configEntryIn = " " + configEntryIn.Trim() + " ";
            string configEntryTo = MainWindow.Configurations.Where(x => x.Key.Equals("ToTranslation")).FirstOrDefault().Value;
            configEntryTo = " " + configEntryTo.Trim() + " ";
            int endPosition, startPosition;
            if (logEntry.Contains(configEntryIn))
            {
                endPosition = logEntry.IndexOf(soldToId, 0) - 1;
                startPosition = logEntry.IndexOf(configEntryTo, 0) + 4;
                return logEntry.Substring(startPosition, endPosition - startPosition);
            }
            else
            {
                return "";
            }
        }

        private static string getShipID(string logEntry)
        {
            string configEntry = MainWindow.Configurations.Where(x => x.Key.Equals("SoldTranslation")).FirstOrDefault().Value;
            configEntry = " " + configEntry.Trim() + " ";
            int position;
            if (logEntry.Contains(configEntry))
            {
                position = logEntry.IndexOf(configEntry, 0);
                return logEntry.Substring(position - 7, 7);
            }
            else
            {
                return "";
            }
        }

        private static string getShipName(string logEntry, string shipID)
        {
            string configEntry = MainWindow.Configurations.Where(x => x.Key.Equals("SoldTranslation")).FirstOrDefault().Value;
            configEntry = " " + configEntry.Trim() + " ";
            int position;
            if (logEntry.Contains(configEntry))
            {
                position = logEntry.IndexOf(shipID, 0);
                return logEntry.Substring(0, position - 1);
            }
            else
            {
                return "";
            }
        }

        private static int getQtdSold(string logEntry)
        {
            string configEntry = MainWindow.Configurations.Where(x => x.Key.Equals("SoldTranslation")).FirstOrDefault().Value;
            configEntry = " " + configEntry.Trim() + " ";
            int position, positionEndQtd;
            if (logEntry.Contains(configEntry))
            {
                position = logEntry.IndexOf(configEntry, 0) + 6;
                positionEndQtd = logEntry.IndexOf(" ", position);
                return int.Parse(logEntry.Substring(position, positionEndQtd - position));
            }
            else
            {
                return 0;
            }
        }

        private static string getProduct(string logEntry, int qtdSold)
        {
            string configEntrySold = MainWindow.Configurations.Where(x => x.Key.Equals("SoldTranslation")).FirstOrDefault().Value;
            configEntrySold = " " + configEntrySold.Trim() + " ";
            string configEntryTo = MainWindow.Configurations.Where(x => x.Key.Equals("ToTranslation")).FirstOrDefault().Value;
            configEntryTo = " " + configEntryTo.Trim() + " ";
            int position, positionStartProduct;
            if (logEntry.Contains(configEntryTo))
            {
                positionStartProduct = logEntry.IndexOf((configEntrySold), 0) + 6 + qtdSold.ToString().Length + 1;
                position = logEntry.IndexOf(configEntryTo, positionStartProduct);

                return logEntry.Substring(positionStartProduct, position - positionStartProduct);
            }
            else
            {
                return "";
            }
        }


    }
}


