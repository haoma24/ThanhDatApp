using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThanhDatApp.BUS
{
    internal class ProductService
    {
        private thanhdatEntities _db;
        public ProductService()
        {
            _db = new thanhdatEntities();
        }
        public List<Products> Get(string id)
        {
            if (id == null)
            {
                return _db.Products.ToList();
            }
            return _db.Products.Where(ac => ac.ProductID == id).ToList();
        }
    }
}
