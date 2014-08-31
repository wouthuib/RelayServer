using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using RelayServer.Database.Items;

namespace RelayServer.Database.Players
{
    // Global Inventory Class.
    // Sealed modifier prevents other classes from inheriting A : B.
    // Static with get { return instance; } makes the class Global.

    [Serializable]
    public sealed class Inventory
    {

        public List<Item> item_list { get; set; }

        #region constructor

        public Inventory()
        {
            item_list = new List<Item>();
        }
        #endregion

        public void addItem(Item addItem)
        {
            item_list.Add(addItem);
        }

        public void removeItem(int ID)
        {
            var onlyMatch = item_list.First(i => i.itemID == ID);
            item_list.Remove(onlyMatch);
        }

        public void loadItems(string file)
        {
            string dir = @"..\..\..\..\XNA_ScreenManagerContent\itemDB";
            string serializationFile = Path.Combine(dir, file);

            //deserialize
            using (Stream stream = File.Open(serializationFile, FileMode.Open))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                item_list = (List<Item>)bformatter.Deserialize(stream);
            }
        }

        public void saveItem(string file)
        {
            string dir = @"..\..\..\..\XNA_ScreenManagerContent\itemDB";
            string serializationFile = Path.Combine(dir, file);

            //serialize
            using (Stream stream = File.Open(serializationFile, FileMode.Create))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                bformatter.Serialize(stream, item_list);
            }
        }
    }
}
