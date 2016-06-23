using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SupplyMe.Models
{
    public class OrderReport
    {
        public int OrderId { get; set; }
        public List<OrderReportDetail> OrderReportDetails { get; set; }
    }
}