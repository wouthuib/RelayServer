using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelayServer.PlayerClasses
{
    public class PlayerStore
    {
        private static PlayerStore instance;
        private int maxplayers = 10, playercounter;
        public playerData[] playerStore;

        private PlayerStore()
        {
            playerStore = new playerData[maxplayers];
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

        public void addPlayer(playerData player)
        {
            if (playercounter < maxplayers)
            {
                playerStore[playercounter] = player;
                playercounter++;
            }
        }
    }
}
