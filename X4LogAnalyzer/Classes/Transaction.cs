using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X4LogAnalyzer.Classes
{
    class Transaction
    {
        public double Time { get; set; }
        public Ship Seller { get; set; }
        public Ship Buyer { get; set; }
        public double Value { get; set; }
        public Ware Ware { get; set; }
        public int Quantity { get; set; }
        public double ItemValue
        {
            get
            {
                return Value / Quantity;
            }
        }
        public string Location { get; set; }
    }
}
