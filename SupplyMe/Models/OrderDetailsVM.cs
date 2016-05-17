using DataLayer.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SupplyMe.Models
{
    public class OrderDetailsVM
    {
        public int OrderDetailId { get; set; }
        public int ItemId { get; set; }
        public Nullable<int> UnitId { get; set; }
        public int Qty { get; set; }

        public OrderDetail toModel()
        {
            OrderDetail detail = new OrderDetail();
            detail.OrderDetailsId = this.OrderDetailId;
            detail.OrderItemId = this.ItemId;
            detail.OrderUnitId = this.UnitId;
            detail.OrderQty = this.Qty;
            return detail;
        }
    }
}