using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThanhDatApp.BUS
{
    internal class BranchService
    {
        private thanhdatEntities _db;
        public BranchService()
        {
            _db = new thanhdatEntities();
        }
        public List<Branches> Get(string id)
        {
            if (id == null)
            {
                return _db.Branches.ToList();
            }
            return _db.Branches.Where(ac => ac.BranchID == id).ToList();
        }
    }
}
