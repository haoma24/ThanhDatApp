using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThanhDatApp.BUS
{
    internal class CategoryService
    {
        private thanhdatEntities _db;
        public CategoryService()
        {
            _db = new thanhdatEntities();
        }
        public List<Categories> Get(string id)
        {
            if (id == null)
            {
                return _db.Categories.ToList();
            }
            return _db.Categories.Where(ac => ac.CategoryID == id).ToList();
        }
    }
}
