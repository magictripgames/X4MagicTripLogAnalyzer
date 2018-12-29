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
    public partial class TabControl : UserControl
    {
        public TabControl()
        {
            InitializeComponent();
        }
        private void TabablzControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ("Ship".Equals(((object[])e.AddedItems)[0].GetType().Name))
                {
                    //This is required when the inner textlist is been selected
                    return;
                }
                if ("tabImportLog".Equals(((System.Windows.FrameworkElement)((object[])e.AddedItems)[0]).Name))
                {
                    Console.WriteLine("tabImportLog got focus");
                    //((Mah)e.AddedItems[0]).UserControl_Loaded(sender, e);
                }
                if ("tabShipInformation".Equals(((System.Windows.FrameworkElement)((object[])e.AddedItems)[0]).Name))
                {
                    Console.WriteLine("tabShipInformation got focus");
                }
                if ("tabAnalysis".Equals(((System.Windows.FrameworkElement)((object[])e.AddedItems)[0]).Name))
                {
                    Console.WriteLine("tabAnalysis got focus");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Exception on TabablzControl_SelectionChanged");
                /*throw*/
                ;
            }
        }

        private void TabablzControl_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("TabablzControl_SelectionChanged");
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("TabablzControl_SelectionChanged");
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Console.WriteLine("TabablzControl_SelectionChanged");
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("TabablzControl_SelectionChanged");
        }

        private void UserControl_IsVisibleChanged_1(object sender, DependencyPropertyChangedEventArgs e)
        {
            Console.WriteLine("TabablzControl_SelectionChanged");
        }
    }
}
