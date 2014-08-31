using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelayServer.Database.Accounts
{
    public class Account
    {
        public long AccountID;
        public string Username;
        public string Password;
        public string IpAddress;

        public static Account create(long ID, string name, string password)
        {
            var results = new Account();
            results.AccountID = ID;
            results.Username = name;
            results.Password = password;
            return results;
        }
    }
}
