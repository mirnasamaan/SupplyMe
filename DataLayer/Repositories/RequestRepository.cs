using DataLayer.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Repositories
{
    public class RequestRepository:BaseRepository<Order>
    {
        public Order AddOrder(Order order)
        {
            Context.Orders.Add(order);
            Context.SaveChanges();
            return order;
        }

        public IQueryable<Category> GetParentCategories()
        {
            return Context.Categories.Where(i => i.CategoryParentId == null);
        }

        public IQueryable<Unit> GetUnitsByItemId(int itemId)
        {
            return Context.Units.Where(i => i.Items.Select(j => j.ItemId).Contains(itemId));
        }

        public Item GetItemById(int itemId)
        {
            if (Context.Items.Any(i => i.ItemId == itemId))
            {
                return Context.Items.FirstOrDefault(i => i.ItemId == itemId);
            }
            else
            {
                return null;
            }
        }

        public Unit GetUnitById(int unitId)
        {
            if (Context.Units.Any(i => i.UnitId == unitId))
            {
                return Context.Units.FirstOrDefault(i => i.UnitId == unitId);
            }
            else
            {
                return null;
            }
        }

    }
}
