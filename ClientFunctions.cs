using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using RelayServer.WorldObjects.Structures;
using System.Xml.Serialization;
using RelayServer.ClientObjects;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml;
using RelayServer.Database.Players;
using RelayServer.WorldObjects;
using RelayServer.WorldObjects.Entities;
using System.Threading.Tasks;
using System.Threading;

namespace RelayServer
{
    public class ClientFunctions
    {
        //Encapsulated 
        protected Client user;

        //Account Autentication
        public long AccountID;
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
                    Thread.Sleep(100);
                }

            // send players
            for (int pl = 0; pl < PlayerStore.Instance.playerStore.Count; pl++ )
                if (PlayerStore.Instance.playerStore[pl].Online)
                {
                    Server.singleton.SendObject(PlayerStore.Instance.toPlayerData(PlayerStore.Instance.playerStore[pl]), user);
                    Thread.Sleep(100);
                }
        }
    }
}
