using DataLayer.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SupplyMe.Models
{
    public class OrderVM
    {
        public int OrderID { get; set; }
        public List<OrderDetailsVM> OrderDetails { get; set; }
        public string OrderMessage { get; set; }

        public OrderVM()
        {
            OrderDetails = new List<OrderDetailsVM>();
        }

        public Order toModel() {
            Order order = new Order();
            order.OrderId = this.OrderID;
            order.OrderMessage = this.OrderMessage;
            foreach (var item in this.OrderDetails)
            {
                order.OrderDetails.Add(item.toModel());
            }
            return order;
        }
    }

}