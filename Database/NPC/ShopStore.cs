using System.Collections.Generic;
using System.IO;
using System;
using RelayServer.Database.Monsters;
using RelayServer.Database.Items;

namespace RelayServer.Database.NPC
{
    public sealed class ShopStore
    {
        public List<Shop> shop_list { get; set; }

        private static ShopStore instance;
        private ShopStore()
        {
            shop_list = new List<Shop>();
        }

        public static ShopStore Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ShopStore();
                }
                return instance;
            }
        }

        public void addShop(Shop addShop)
        {
            shop_list.Add(addShop);
        }

        public void removeShop(int ID)
        {
            shop_list.Remove(new Shop() { ShopID = ID });
        }

        public Shop getShop(int ID)
        {
            return this.shop_list.Find(delegate(Shop shop) { return shop.ShopID == ID; });
        }

        public void loadShop(string dir, string file)
        {
            using (var reader = new StreamReader(Path.Combine(dir, file)))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    try
                    {
                        this.addShop(Shop.create(Convert.ToInt32(values[0])));
                        Shop shop = this.getShop(Convert.ToInt32(values[0]));

                        // here we are reading the itemlist
                        for (int i = 1; i < values.Length; i++)
                            if ((values[i] != null) && 
                                ItemStore.Instance.item_list.FindAll(x => x.itemID == Convert.ToInt32(values[i])).Count > 0)
                                    shop.shop_list.Add(ItemStore.Instance.item_list.Find(x => x.itemID == Convert.ToInt32(values[i])));
                    }
                    catch (Exception ee)
                    {
                        string exception = ee.ToString();
                    }
                }
            }
        }
    }
}
