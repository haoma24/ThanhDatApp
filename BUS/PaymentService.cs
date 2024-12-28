using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThanhDatApp.BUS
{
    internal class PaymentService
    {
        private thanhdatEntities _db;
        public PaymentService()
        {
            _db = new thanhdatEntities();
        }
        public List<Payment> Get(string id)
        {
            if (id == null)
            {
                return _db.Payment.ToList();
            }
            return _db.Payment.Where(ac => ac.PaymentID == id).ToList();
        }
    }
}
