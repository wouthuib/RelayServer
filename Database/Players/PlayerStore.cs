using System;
using System.Collections.Generic;
using RelayServer.Database.Players;
using Microsoft.Xna.Framework;
using RelayServer.Database.Items;
using System.IO;
using System.Xml.Serialization;
using RelayServer.Database.Accounts;
using MapleLibrary;

namespace RelayServer.Database.Players
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

            if (playerStore.FindAll(x => x.Name == playerdata.Name).Count == 0) // avoid duplicates
            {
                player.AccountID = playerdata.AccountID;
                player.CharacterID = playerStore.Count + 2000000;

                player.Name = playerdata.Name;
                player.skin_color = getColor(playerdata.skincol);
                player.faceset_sprite = playerdata.facespr;
                player.hair_sprite = playerdata.hairspr;
                player.hair_color = getColor(playerdata.hailcol);

                player.equipment.addItem(ItemStore.Instance.item_list.Find(x => x.itemName == playerdata.armor));
                player.equipment.addItem(ItemStore.Instance.item_list.Find(x => x.itemName == playerdata.weapon));

                playerStore.Add(player);
                savePlayerStore(); // save to XML (later to SQL ;-))
            }
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
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PlayerInfo>));
                xmlSerializer.Serialize(fs, playerStore);
            }
        }

        public void loadPlayerStore()
        {
            string dir = Directory.GetCurrentDirectory() + @"\Import\";
            string serializationFile = Path.Combine(dir, "character.xml");

            using (var fs = new FileStream(serializationFile, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<PlayerInfo>));
                playerStore = (List<PlayerInfo>)serializer.Deserialize(fs);
            }
        }

        public playerData toPlayerData(PlayerInfo player)
        {
            playerData p = new playerData()
            {
                  Name  = player.Name,
                  AccountID  = player.AccountID,
                  CharacterID  = player.CharacterID,
                  PositionX  = Convert.ToInt32(player.Position.X),
                  PositionY  = Convert.ToInt32(player.Position.Y),
                  skincol  = player.skin_color.ToString(),
                  facespr  = player.faceset_sprite,
                  hairspr  = player.hair_sprite,
                  hailcol  = player.hair_color.ToString()
            };

            //if (AccountStore.Instance.account_list.FindAll(x => x.AccountID == player.AccountID).Count != 0)
            //    p.IP = AccountStore.Instance.account_list.Find(x => x.AccountID == player.AccountID).IpAddress.ToString();

            if (player.equipment.item_list.FindAll(x=>x.Slot == ItemSlot.Bodygear).Count > 0)
                p.armor = player.equipment.item_list.Find(x => x.Slot == ItemSlot.Bodygear).itemName;
            if (player.equipment.item_list.FindAll(x => x.Slot == ItemSlot.Headgear).Count > 0)
                p.headgear = player.equipment.item_list.Find(x => x.Slot == ItemSlot.Headgear).itemName;
            if (player.equipment.item_list.FindAll(x => x.Slot == ItemSlot.Weapon).Count > 0)
                p.weapon = player.equipment.item_list.Find(x => x.Slot == ItemSlot.Weapon).itemName;

            return p;
        }
    }
}
