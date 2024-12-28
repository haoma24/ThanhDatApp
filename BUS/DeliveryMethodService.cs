using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThanhDatApp.BUS
{
    internal class DeliveryMethodService
    {
        private thanhdatEntities _db;
        public DeliveryMethodService()
        {
            _db = new thanhdatEntities();
        }
        public List<DeliveryMethod> Get(string id)
        {
            if (id == null)
            {
                return _db.DeliveryMethod.ToList();
            }
            return _db.DeliveryMethod.Where(ac => ac.DeliveryMethodID == id).ToList();
        }
    }
}
