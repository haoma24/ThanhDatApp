using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThanhDatApp.BUS
{
    internal class AccountService
    {
        private thanhdatEntities _db;
        public AccountService()
        {
            _db = new thanhdatEntities();
        }
        public List<Accounts> Get(string id)
        {
            if (id == null)
            {
                return _db.Accounts.ToList();
            }
            return _db.Accounts.Where(ac => ac.AccountID == id).ToList();
        }
        public string GetId (string TK, string MK)
        {
            return _db.Accounts
                .Where(ac => ac.UserName == TK || ac.Email == TK && ac.PasswordHash == MK)
                .Select(ac=>ac.AccountID) .FirstOrDefault();
        }
        public bool isExist (string TK, string MK)
        {
            return _db.Accounts.Where(ac=>ac.UserName == TK || ac.Email==TK && ac.PasswordHash==MK).Any();
        }
    }
}
