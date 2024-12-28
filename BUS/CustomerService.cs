using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThanhDatApp.BUS
{
    internal class CustomerService
    {
        private thanhdatEntities _db;
        public CustomerService()
        {
            _db = new thanhdatEntities();
        }
        public List<Customers> Get(string id)
        {
            if (id == null)
            {
                return _db.Customers.ToList();
            }
            return _db.Customers.Where(e => e.CustomerID == id).ToList();
        }
    }
}
