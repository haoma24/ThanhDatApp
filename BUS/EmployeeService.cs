using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThanhDatApp.BUS
{
    internal class EmployeeService
    {
        private thanhdatEntities _db;
        public EmployeeService() 
        {
            _db = new thanhdatEntities();
        }
        public List<Employees> Get(string id)
        {
            if (id == null)
            {
                return _db.Employees.ToList();
            }
            return _db.Employees.Where(e => e.EmployeeID == id).ToList();
        }
    }
}
