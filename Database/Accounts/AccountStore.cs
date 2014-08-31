using System.Collections.Generic;
using System.IO;
using System;

namespace RelayServer.Database.Accounts
{
    public sealed class AccountStore
    {
        public List<Account> account_list { get; set; }

        private static AccountStore instance;
        private AccountStore()
        {
            account_list = new List<Account>();
        }

        public static AccountStore Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AccountStore();
                }
                return instance;
            }
        }

        public void addAccount(Account addAccount)
        {
            if (addAccount.AccountID == 0)
                addAccount.AccountID = account_list.Count + 1000000;

            account_list.Add(addAccount);
        }

        public void removeAccount(int ID)
        {
            account_list.Remove(new Account() { AccountID = ID });
        }

        public Account getAccount(int ID)
        {
            return this.account_list.Find(x => x.AccountID == ID);
        }

        public long FindAccount(string username, string password)
        {
            // update memory with new accounts
            loadAccounts(Directory.GetCurrentDirectory() + @"\Import\", "accounttable.csv");

            // read memory
            foreach (var entry in account_list)
            {
                if (entry != null)
                {
                    if (entry.Username == username &&
                        entry.Password == password)
                    {
                        return entry.AccountID;
                    }
                }
            }

            return 0;
        }

        public void loadAccounts(string dir, string file)
        {
            using (var reader = new StreamReader(Path.Combine(dir, file)))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    try
                    {
                        if (account_list.FindAll(x => x.AccountID == Convert.ToInt64(values[0])).Count == 0)
                            this.addAccount(Account.create(Convert.ToInt64(values[0]), values[1], values[2]));
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
