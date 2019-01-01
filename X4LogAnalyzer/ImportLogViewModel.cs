using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using X4LogAnalyzer;

namespace X4LogAnalyzer
{
    public class ImportLogViewModel
    {
        public ICommand ShowInputDialogCommand { get; }

        public ICommand ClearHistoryCommand { get; }

        public ICommand ShowProgressDialogCommand { get; }

        public ICommand ShowLeftFlyoutCommand { get; }

        private static List<TradeOperation> tradeOperations = new List<TradeOperation>();

        private ResourceDictionary DialogDictionary = new ResourceDictionary() { Source = new Uri("pack://application:,,,/MaterialDesignThemes.MahApps;component/Themes/MaterialDesignTheme.MahApps.Dialogs.xaml") };

        public ImportLogViewModel()
        {
            ShowInputDialogCommand = new AnotherCommandImplementation(_ => InputDialog());

            ClearHistoryCommand = new AnotherCommandImplementation(_ => ClearJsonHistory());

            //ShowProgressDialogCommand = new AnotherCommandImplementation(_ => ProgressDialog());
            //ShowLeftFlyoutCommand = new AnotherCommandImplementation(_ => ShowLeftFlyout());
        }

        private void ClearJsonHistory()
        {
            string applicationPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            var directory = System.IO.Path.GetDirectoryName(applicationPath).Remove(0, 6);
            string fileName = directory + @"\X4LogAnalyzerTempXML.json";
            File.Delete(fileName);
            MainWindow.GlobalTradeOperations.Clear();
            MainWindow.ShipsWithTradeOperations.Clear();
            MainWindow.GlobalWares.Clear();
        }

        public Flyout LeftFlyout { get; set; }

        private async void InputDialog()
        {
            try
            {
                var metroDialogSettings = new MetroDialogSettings
                {
                    CustomResourceDictionary = DialogDictionary,
                    DefaultText = MainWindow.Configurations.Where(x => x.Key.Equals("LastSaveGameLoaded")).FirstOrDefault().Value,
                    NegativeButtonText = "CANCEL"
                };

                string filePath = await DialogCoordinator.Instance.ShowInputAsync(this, "Select log file to be imported", "File:", metroDialogSettings);
                if (filePath == null || "".Equals(filePath))
                {
                    return;
                }
                string path = Environment.ExpandEnvironmentVariables(filePath);
                MainWindow.Configurations.Where(x => x.Key.Equals("LastSaveGameLoaded")).FirstOrDefault().Value = path;

                MainWindow.SaveConfigurations();

                Console.WriteLine(@path);
                FileInfo compressedSaveFile = new FileInfo(path);
                Decompress(compressedSaveFile);
            }
            catch (Exception err)
            {

                MessageBox.Show(string.Format("Error found while trying to import the save game. Error description: {0}", err.Message));
            }
            
        }

        private async void ProgressDialog()
        {
            var metroDialogSettings = new MetroDialogSettings
            {
                CustomResourceDictionary = DialogDictionary,
                NegativeButtonText = "CANCEL"
            };

            var controller = await DialogCoordinator.Instance.ShowProgressAsync(this, "MahApps Dialog", "Using Material Design Themes (WORK IN PROGRESS)", true, metroDialogSettings);
            controller.SetIndeterminate();
            await Task.Delay(3000);
            await controller.CloseAsync();
        }

        public async Task<MessageDialogResult> ShowMessage(string message, MessageDialogStyle dialogStyle)
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            metroWindow.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;

            return await metroWindow.ShowMessageAsync("MY TITLE", message, dialogStyle, metroWindow.MetroDialogOptions);
        }

        private void ShowLeftFlyout()
        {
            ((MainWindow)Application.Current.MainWindow).LeftFlyout.IsOpen = !((MainWindow)Application.Current.MainWindow).LeftFlyout.IsOpen;
        }

        public static void Decompress(FileInfo fileToDecompress)
        {
            string currentFileName = fileToDecompress.FullName;
            string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);
            string applicationPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            var directory = System.IO.Path.GetDirectoryName(applicationPath).Remove(0,6);

            MainWindow.DeserializeWares(directory);

            newFileName = directory + @"\X4LogAnalyzerTempXML.xml";
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                NewMethod(fileToDecompress, newFileName, originalFileStream);

            }
            FileStream decompressedSaveFile = new FileStream(newFileName, FileMode.Open);
            XmlReader xmlSave = XmlReader.Create(decompressedSaveFile);
            
            while (xmlSave.Read())
            {
                if (xmlSave.NodeType == XmlNodeType.Element)
                {
                    if (xmlSave.Name == "entry" || xmlSave.Name == "entry")
                    {
                        if (xmlSave.HasAttributes)
                        {
                            writeLogEntry(xmlSave);
                        }
                    }
                    
                }
                //else if (xmlSave.NodeType == XmlNodeType.Text)
                //{
                //    Console.WriteLine("\tVALUE: " + xmlSave.Value);
                //}
            }
            
            foreach (TradeOperation tradeOp in tradeOperations)
            {
                try
                {
                    MainWindow.GlobalTradeOperations.Add(tradeOp);
                    MainWindow.AddTradeOperationToShipList(tradeOp);
                    MainWindow.AddTradeOperationToWareList(tradeOp);
                    tradeOp.WriteToLog();
                }
                catch (Exception err)
                {

                    MessageBox.Show(string.Format("Error while trying to summarize the operations - {0}",err.Message));
                }
                
            }
            using (StreamWriter file = File.CreateText(directory + @"\X4LogAnalyzerTempXML.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, MainWindow.GlobalTradeOperations);
                file.Close();
            }
            
            //string json = JsonConvert.SerializeObject(MainWindow.GlobalTradeOperations);
            //File.WriteAllText(directory + @"\X4LogAnalyzerTempXML.json", json);

            decompressedSaveFile.Close();
            Thread newThread = new Thread(ImportLogViewModel.Delete);
            newThread.IsBackground = true;
            newThread.Start(new FileInfo(newFileName));
            //Delete(new FileInfo(newFileName));
            //File.Delete(newFileName);
        }

        private static void NewMethod(FileInfo fileToDecompress, string newFileName, FileStream originalFileStream)
        {
            try
            {
                using (FileStream decompressedFileStream = File.Create(@newFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                        Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
                        decompressionStream.Close();
                    };


                    decompressedFileStream.Flush();
                    decompressedFileStream.Close();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(string.Format("Error trying to open the file {0}. This error may occur when the Temporary file was not deleted previously or the application does not have permission to delete it. {1}",@newFileName, @err.Message));
            }
            
        }

        private static void writeLogEntry(XmlReader logEntry)
        {
            logEntry.MoveToNextAttribute();
            //List<TradeOperation> tradeOperations = new List<TradeOperation>();
            
            bool isATradeOperation = false;
            if ("time".Equals(logEntry.Name))
            {
                if (MainWindow.GlobalTradeOperations.Find(x => x.Time == double.Parse(logEntry.Value)) != null)
                {
                    //This item already exists in the list
                    return;
                }
                TradeOperation currentTradeOperation = new TradeOperation(double.Parse(logEntry.Value));
                //Console.WriteLine(string.Format("\t{0} : {1}", logEntry.Name, logEntry.Value));
                while (logEntry.MoveToNextAttribute())
                {
                    //Console.WriteLine(string.Format("\t{0} : {1}", logEntry.Name, logEntry.Value));
                    if ("title".Equals(logEntry.Name) && MainWindow.Configurations.Where(x => x.Key.Equals("TradeCompletedTranslation")).FirstOrDefault().Value.Equals(logEntry.Value))
                    {
                        isATradeOperation = true;

                    }
                    if (isATradeOperation && "text".Equals(logEntry.Name))
                    {
                        currentTradeOperation.ParseTextEntry(logEntry);
                    }
                    if (isATradeOperation && "faction".Equals(logEntry.Name))
                    {
                        currentTradeOperation.Faction = logEntry.Value;
                        //Console.WriteLine(string.Format("\t{0} : {1}", "Faction", logEntry.Value));
                    }
                    if (isATradeOperation && "time".Equals(logEntry.Name))
                    {
                        //Console.WriteLine(string.Format("\t{0} : {1}", "Time", logEntry.Value));
                        //currentTradeOperation = new TradeOperation(float.Parse(logEntry.Value));
                        //tradeOperations.Add(currentTradeOperation);
                    }
                    if (isATradeOperation && "money".Equals(logEntry.Name))
                    {
                        int money = int.Parse(logEntry.Value);
                        money = (money / 100);
                        currentTradeOperation.Money = money;
                        //Console.WriteLine(string.Format("\t{0} : {1}", "money", money));
                    }
                }
                if (isATradeOperation)
                {
                    
                    tradeOperations.Add(currentTradeOperation);
                    currentTradeOperation.OurShip.AddTradeOperation(currentTradeOperation);
                    currentTradeOperation.SoldTo.AddTradeOperation(currentTradeOperation);
                    currentTradeOperation.PartialSumByShip = currentTradeOperation.OurShip.GetListOfTradeOperations().Sum(x => x.Money);

                }
            }
        }

        

        public static void Delete(object file)
        {
            FileInfo fileInfo = (FileInfo)file;
            if (fileInfo.Exists)
            {
                int Attempt = 0;
                bool ShouldStop = false;
                while (!ShouldStop)
                {
                    if (CanDelete(fileInfo))
                    {
                        fileInfo.Delete();
                        ShouldStop = true;
                    }
                    else if (Attempt >= 3)
                    {
                        ShouldStop = true;
                    }
                    else
                    {
                        // wait one sec
                        System.Threading.Thread.Sleep(1000);
                    }

                    Attempt++;
                }
            }
        }

        private static bool CanDelete(FileInfo file)
        {
            try
            {
                //Just opening the file as open/create
                using (FileStream fs = new FileStream(file.FullName, FileMode.OpenOrCreate))
                {
                    //If required we can check for read/write by using fs.CanRead or fs.CanWrite
                    fs.Close();
                    return true;
                }
                //return false;
            }
            catch (IOException ex)
            {
                //check if message is for a File IO
                string __message = ex.Message.ToString();
                if (__message.Contains("The process cannot access the file"))
                    return false;
                else
                    throw;
            }
        }

    }
}
