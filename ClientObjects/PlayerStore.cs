using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelayServer.ClientObjects
{
    public class PlayerStore
    {
        private static PlayerStore instance;
        public List<playerData> playerStore = new List<playerData>();

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

        public void addPlayer(playerData player)
        {
            playerStore.Add(player);
        }
    }
}
