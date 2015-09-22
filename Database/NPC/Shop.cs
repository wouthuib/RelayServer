using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RelayServer.Database.Items;

namespace RelayServer.Database.NPC
{
    public class Shop
    {
        public long ShopID;
        public List<Item> shop_list { get; set; }

        public static Shop create(long ID)
        {
            var results = new Shop();
            results.ShopID = ID;
            results.shop_list = new List<Item>();
            return results;
        }
    }
}
