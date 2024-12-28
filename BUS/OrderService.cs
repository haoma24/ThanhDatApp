using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThanhDatApp.BUS
{
    internal class OrderService
    {
        private thanhdatEntities _db;
        public OrderService()
        {
            _db = new thanhdatEntities();
        }
        public List<Orders> Get(string id)
        {
            if (id == null)
            {
                return _db.Orders.ToList();
            }
            return _db.Orders.Where(e => e.OrderID == id).ToList();
        }
    }
}
