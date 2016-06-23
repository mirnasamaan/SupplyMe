using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SupplyMe.Models
{
    public class OrderReportDetail
    {
        public string CategoryName { get; set; }
        public string ItemName { get; set; }
        public int Qty { get; set; }
        public string UnitName { get; set; }
        public string OrderMessage { get; set; }
    }
}