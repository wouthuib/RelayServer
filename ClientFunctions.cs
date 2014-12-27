using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using RelayServer.WorldObjects.Structures;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml;
using RelayServer.Database.Players;
using RelayServer.WorldObjects;
using RelayServer.WorldObjects.Entities;
using System.Threading.Tasks;
using System.Threading;
using RelayServer.Database.Accounts;
using MapleLibrary;
using RelayServer.Database.Items;

namespace RelayServer
{
    public class ClientFunctions
    {
        //Encapsulated 
        protected Client user;

        //Account Autentication
        public long AccountID;
        public long CharacterID;
        public bool Autenticated = false;

        //Screen Management 
        public string MainScreenName = "login";
        public string MainScreenPhase = "";
        public string MainScreenMenu = "";
        public string SubScreenName = "";
        public string SubScreenPhase = "";
        public string SubScreenMenu = "";

        //Is this client disconnected?
        protected bool connected = false;
        protected int logoutput = 0;

        public ClientFunctions()
        {
            connected = true;
        }
    }

    public class clientfunction
    {
        public static void loadmap(Client user)
        {
            // send monsters
            for (int et = 0; et < GameWorld.Instance.listEntity.Count; et++ )
                if (GameWorld.Instance.listEntity[et] is MonsterSprite)
                {
                    MonsterSprite monster = (MonsterSprite)GameWorld.Instance.listEntity[et];
                    monster.sendtoClient(user);
                }

            // send players
            for (int pl = 0; pl < PlayerStore.Instance.playerStore.Count; pl++ )
                if (PlayerStore.Instance.playerStore[pl].Online)
                {
                    Server.singleton.SendObject(PlayerStore.Instance.toPlayerData(PlayerStore.Instance.playerStore[pl]), user);
                }
        }
        
        public static void updateHUD(PlayerInfo player, HudData huddata)
        {
            Client user = null;

            user = Array.Find(Server.singleton.client, x => x.CharacterID == player.CharacterID);

            Server.singleton.SendObject(huddata, user);
        }

        public static void updateInventory(Client user)
        {
            ItemData i = null;

            // reset inventory at client
            Server.singleton.SendObject(
                new ItemData()
                {
                    ID = 0,
                    action = "ResInventory"
                },
                    user);

            // send inventory items to client
            foreach (var item in PlayerStore.Instance.playerStore.Find(x => x.CharacterID == user.CharacterID).inventory.item_list)
            {
                i = new ItemData()
                {
                    ID = item.itemID,
                    action = "AddInventory"
                };

                Server.singleton.SendObject(i, user);
            }

            // send finish inventory to client
            i = new ItemData()
            {
                ID = 0,
                action = "FinInventory"
            };

            Server.singleton.SendObject(i, user);
        }

        public static void updateEquipment(Client user)
        {
            ItemData i = null;
            string playername = null;

            if (PlayerStore.Instance.playerStore.FindAll(x => x.CharacterID == user.CharacterID).Count > 0)
                playername = PlayerStore.Instance.playerStore.Find(x => x.CharacterID == user.CharacterID).Name;

            // reset equipment at client
            Server.singleton.SendObject(
                new ItemData()
                {
                    ID = 0,
                    action = "ResEquipment",
                    player_name = playername
                },
                    user);

            // send equipment items to client
            foreach (var item in PlayerStore.Instance.playerStore.Find(x => x.CharacterID == user.CharacterID).equipment.item_list)
            {
                i = new ItemData()
                {
                    ID = item.itemID,
                    action = "AddEquipment",
                    player_name = playername
                };

                Server.singleton.SendObject(i, user);

                // send new equipment info to other clients
                i = new ItemData()
                {
                    ID = item.itemID,
                    action = "SwitchEquipment",
                    player_name = playername
                };

                Server.singleton.SendObject(i); 
            }

            // send finish equipment to client
            i = new ItemData()
            {
                ID = 0,
                action = "FinEquipment",
                player_name = playername
            };

            Server.singleton.SendObject(i, user);            
        }

        public static void EquipItem(Client user, ItemData itemdata)
        {
            Item item = null;
            PlayerInfo player = null;

            // check item
            if (ItemStore.Instance.item_list.FindAll(x => x.itemID == itemdata.ID).Count > 0)
                item = ItemStore.Instance.item_list.Find(x => x.itemID == itemdata.ID);

            // check player
            if (PlayerStore.Instance.playerStore.FindAll(x => x.CharacterID == user.CharacterID).Count > 0)
                player = PlayerStore.Instance.playerStore.Find(x => x.CharacterID == user.CharacterID);

            // check inventory
            if (PlayerStore.Instance.playerStore.Find(x => x.CharacterID == user.CharacterID).
                inventory.item_list.FindAll(x => x.itemID == itemdata.ID).Count > 0 && item != null)
            {
                if ((int)item.Type > 0)
                {
                    if (player.equipment.getEquip(item.Slot) == null)
                    {
                        player.equipment.addItem(item);
                        player.inventory.removeItem(item.itemID);
                    }
                    else
                    {
                        Item getequip = player.equipment.getEquip(item.Slot);

                        player.equipment.removeItem(item.Slot);
                        player.equipment.addItem(item);

                        player.inventory.removeItem(item.itemID);
                        player.inventory.addItem(getequip);

                        // send old equipment to client itemscreen
                        Server.singleton.SendObject(
                            new ItemData()
                                {
                                    ID = item.itemID,
                                    action = "AddInventory"
                                },
                                user);
                    }

                    // update client player equipment
                    clientfunction.updateEquipment(user);

                    // update client player inventory
                    clientfunction.updateInventory(user);
                }
            }
        }

        public static void AddItem(Client user, ItemData itemdata)
        {
            Item item = null;
            PlayerInfo player = null;

            // check item
            if (ItemStore.Instance.item_list.FindAll(x => x.itemID == itemdata.ID).Count > 0)
                item = ItemStore.Instance.item_list.Find(x => x.itemID == itemdata.ID);

            // check player
            if (PlayerStore.Instance.playerStore.FindAll(x => x.CharacterID == user.CharacterID).Count > 0)
                player = PlayerStore.Instance.playerStore.Find(x => x.CharacterID == user.CharacterID);

            // check inventory
            player.inventory.addItem(item);

            // send item to client
            Server.singleton.SendObject(
                new ItemData()
                {
                    ID = item.itemID,
                    action = "AddInventory"
                },
                    user);
        }

        public static void saveCharacter(Client user)
        {
            PlayerInfo player = null;

            // check player
            if (PlayerStore.Instance.playerStore.FindAll(x => x.CharacterID == user.CharacterID).Count > 0)
                player = PlayerStore.Instance.playerStore.Find(x => x.CharacterID == user.CharacterID);

            PlayerStore.Instance.savePlayerStore();
        }
    }
}
