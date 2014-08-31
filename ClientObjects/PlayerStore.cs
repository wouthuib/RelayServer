using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RelayServer.Database.Players;
using Microsoft.Xna.Framework;
using RelayServer.Database.Items;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace RelayServer.ClientObjects
{
    public class PlayerStore
    {
        private static PlayerStore instance;
        public List<PlayerInfo> playerStore = new List<PlayerInfo>();

        private PlayerStore()
        {
        }

        public static PlayerStore Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlayerStore();
                }
                return instance;
            }
        }

        public void addPlayer(playerData playerdata)
        {
            PlayerInfo player = new PlayerInfo();

            player.AccountID = playerdata.AccountID;
            player.CharacterID = playerStore.Count + 2000000;

            player.Name = playerdata.Name;
            player.skin_color = getColor(playerdata.skincol);
            player.faceset_sprite = playerdata.facespr;
            player.hair_sprite = playerdata.hairspr;
            player.hair_color = getColor(playerdata.hailcol);

            player.equipment.addItem(ItemStore.Instance.item_list.Find(x => x.itemName == playerdata.armor));
            player.equipment.addItem(ItemStore.Instance.item_list.Find(x => x.itemName == playerdata.headgear));
            player.equipment.addItem(ItemStore.Instance.item_list.Find(x => x.itemName == playerdata.weapon));

            playerStore.Add(player);
            savePlayerStore(); // save to XML
        }

        private Color getColor(string colorcode)
        {
            string[] values = colorcode.Split(':');

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = values[i].Trim(new char[] { ' ', 'R', 'G', 'B', 'A', '{', '}' });
            }

            return new Color(
                Convert.ToInt32(values[1]),
                Convert.ToInt32(values[2]),
                Convert.ToInt32(values[3]));
        }
                
        public void savePlayerStore()
        {
            string dir = Directory.GetCurrentDirectory() + @"\Import\";
            string serializationFile = Path.Combine(dir, "character.xml");

            using (var fs = new FileStream(serializationFile, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(PlayerInfo));
                xmlSerializer.Serialize(fs, playerStore);
            }
        }

        public void loadPlayerStore()
        {
            string dir = Directory.GetCurrentDirectory() + @"\Import\";
            string serializationFile = Path.Combine(dir, "character.xml");

            using (var fs = new FileStream(serializationFile, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PlayerInfo));
                playerStore = (List<PlayerInfo>)serializer.Deserialize(fs);
            }
        }
    }
}
