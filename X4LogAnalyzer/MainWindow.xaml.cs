using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using X4LogAnalyzer;

namespace X4LogAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public class Configuration
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public Configuration()
            {

            }
        }
        //public static ObservableCollection<TradeOperation> GlobalTradeOperations = new ObservableCollection<TradeOperation>();
        public static List<TradeOperation> GlobalTradeOperations = new List<TradeOperation>();
        public static List<Ware> GlobalWares = new List<Ware>();
        public static List<Ship> ShipsWithTradeOperations = new List<Ship>();
        public static List<Ship> DestinationsWithTradeOperations = new List<Ship>();
        public static List<TradeOperation> filteredList = new List<TradeOperation>();
        public static List<Configuration> Configurations = new List<Configuration>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void TabablzControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            

        }

        private void MetroWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            string applicationPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            var directory = System.IO.Path.GetDirectoryName(applicationPath).Remove(0, 6);
            DeserializeConfigurations(directory);
            DeserializeWares(directory);
            DeserializeTradeOperations(directory);

        }

        private void DeserializeTradeOperations(string directory)
        {
            string newFileName = directory + @"\X4LogAnalyzerTempXML.json";
            try
            {
                using (StreamReader r = new StreamReader(newFileName))
                {
                    string json = r.ReadToEnd();
                    GlobalTradeOperations = JsonConvert.DeserializeObject<List<TradeOperation>>(json);
                    foreach (TradeOperation tradeOp in GlobalTradeOperations)
                    {
                        AddTradeOperationToShipList(tradeOp);
                        AddTradeOperationToWareList(tradeOp);
                        //tradeOp.OurShip.AddTradeOperation(tradeOp);
                    }
                    Console.WriteLine(GlobalTradeOperations.Count());
                }
            }
            catch (Exception err)
            {
                //Error opening the history file, ideally I should check if the file exists insted of handling this exception
                Console.WriteLine("Exception on MetroWindow_Loaded");
                Console.WriteLine(err.Message);
            }
        }



        public static void DeserializeConfigurations(string directory)
        {
            string configurationsFileName = directory + @"\Configurations.json";
            try
            {
                using (StreamReader r = new StreamReader(configurationsFileName))
                {
                    string json = r.ReadToEnd();
                    Configurations = JsonConvert.DeserializeObject<List<Configuration>>(json);
                    if (Configurations.Count == 0)
                    {
                        Configurations.Add(new Configuration() { Key = "LastSaveGameLoaded", Value = @" %userprofile%\Documents\Egosoft\X4\[REPLACE_BY_USERID]\save\quicksave.xml.gz" });
                    }
                    //GlobalWares = JsonConvert.DeserializeObject<List<Ware>>(json);
                    Console.WriteLine(Configurations.Count());
                }
            }
            catch (Exception err)
            {

                SaveConfigurations();
                //Error opening the history file, ideally I should check if the file exists insted of handling this exception
                Console.WriteLine("Exception on MetroWindow_Loaded");
                Console.WriteLine(err.Message);
            }
        }

        public static void SaveConfigurations()
        {
            string applicationPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            var directory = System.IO.Path.GetDirectoryName(applicationPath).Remove(0, 6);
            using (StreamWriter file = File.CreateText(directory + @"\Configurations.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                if (MainWindow.Configurations.Where(x => x.Key.Equals("LastSaveGameLoaded")).FirstOrDefault() == null)
                {
                    MainWindow.Configurations.Add(new Configuration() { Key = "LastSaveGameLoaded", Value = @"%userprofile%\Documents\Egosoft\X4\[REPLACE_BY_USERID]\save\quicksave.xml.gz" });
                }
                if (MainWindow.Configurations.Where(x => x.Key.Equals("SoldToTranslation")).FirstOrDefault() == null)
                {
                    MainWindow.Configurations.Add(new Configuration() { Key = "SoldTranslation", Value = @"sold" });
                }
                if (MainWindow.Configurations.Where(x => x.Key.Equals("TradeCompletedTranslation")).FirstOrDefault() == null)
                {
                    MainWindow.Configurations.Add(new Configuration() { Key = "TradeCompletedTranslation", Value = @"Trade Completed" });
                }
                if (MainWindow.Configurations.Where(x => x.Key.Equals("InTranslation")).FirstOrDefault() == null)
                {
                    MainWindow.Configurations.Add(new Configuration() { Key = "InTranslation", Value = @"in" });
                }
                if (MainWindow.Configurations.Where(x => x.Key.Equals("ToTranslation")).FirstOrDefault() == null)
                {
                    MainWindow.Configurations.Add(new Configuration() { Key = "ToTranslation", Value = @"to" });
                }
                
                //serialize object directly into file stream
                serializer.Serialize(file, MainWindow.Configurations);
                file.Close();
            }
        }

        public static void DeserializeWares(string directory)
        {
            List<Ware> TempWares = new List<Ware>();
            string waresFileName = directory + @"\Wares.json";
            try
            {
                using (StreamReader r = new StreamReader(waresFileName))
                {
                    string json = r.ReadToEnd();
                    TempWares = JsonConvert.DeserializeObject<List<Ware>>(json);
                    Console.WriteLine(TempWares.Count());
                }
                foreach (Ware ware in TempWares)
                {
                    Ware globalWare = GlobalWares.Where(x => x.Name.Equals(ware.Name)).FirstOrDefault();
                    if (globalWare == null)
                    {
                        GlobalWares.Add(ware);
                    }
                }
                //GlobalWares = TempWares;
            }
            catch (Exception err)
            {
                //Error opening the history file, ideally I should check if the file exists insted of handling this exception
                Console.WriteLine("Exception on MetroWindow_Loaded");
                Console.WriteLine(err.Message);
            }
        }

        public static void AddTradeOperationToShipList(TradeOperation tradeOp)
        {
            Ship ship = ShipsWithTradeOperations.Where(x => x.FullShipname.Equals(tradeOp.OurShip.FullShipname)).FirstOrDefault();
            if (ship == null)
            {
                ship = tradeOp.OurShip;
                ShipsWithTradeOperations.Add(ship);
            }
            tradeOp.PartialSumByShip = ship.GetListOfTradeOperations().Sum(x => x.Money) + tradeOp.Money;
            ship.AddTradeOperation(tradeOp);
            
        }

        public static void AddTradeOperationToWareList(TradeOperation tradeOp)
        {
            Ware ware = GlobalWares.Where(x => x.Name.Equals(tradeOp.ItemSold.Name)).FirstOrDefault();
            //if (ware == null)
            //{
            //    ware = tradeOp.ItemSold;
            //    ShipsWithTradeOperations.Add(ship);
            //}
            ware.AddTradeOperation(tradeOp);
        }
    }
}
