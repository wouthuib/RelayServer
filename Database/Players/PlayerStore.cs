using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace RelayServer.Database.Players
{
    public class PlayerStore
    {
        public PlayerInfo[] playerlist;
        int activeSlot = 0, count = 0;

        private static PlayerStore instance;
        private PlayerStore()
        {
            playerlist = new PlayerInfo[6];
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

        public int ActiveSlot
        {
            get { return activeSlot; }
            set { activeSlot = value; }
        }

        public int Count
        {
            get { return count; }
            set { count = value; }
        }

        public void addPlayer(PlayerInfo player = null)
        {
            if (player == null)
                playerlist[count] = new PlayerInfo();
            else
                playerlist[count] = player;

            this.count++;
            this.activeSlot = count - 1;
        }

        public void removePlayer(string name, int slot = -1)
        {
            if (slot >= 0)
            {
                playerlist[slot] = null;
                this.count--;
            }

            for (int i = 0; i < playerlist.Length; i++)
            {
                if (playerlist[i].Name == name)
                {
                    playerlist[i] = null;
                    this.count--;
                }
            }

            if (activeSlot > count)
                activeSlot = count - 1;
        }

        public PlayerInfo activePlayer
        {
            get
            {
                if (playerlist[activeSlot] != null)
                    return playerlist[activeSlot];
                else
                    return new PlayerInfo();
            }
        }

        public PlayerInfo getPlayer(string name = null, int slot = -1)
        {
            if (slot >= 0)
                return playerlist[slot];

            for (int i = 0; i < playerlist.Length; i++)
            {
                if (playerlist[i].Name == name)
                    return playerlist[i];
            }

            return null;
        }

        public void loadPlayerStore(string file)
        {
            // unload current players
            for (int i = 0; i < playerlist.Length; i++)
                playerlist[i] = null;

            string dir = @"..\..\..\..\XNA_ScreenManagerContent\playerstore\";
            string serializationFile = Path.Combine(dir, file);

            byte[] key = { 1, 2, 3, 4, 5, 6, 7, 8 }; // Where to store these keys is the tricky part, you may need to obfuscate them or get the user to input a password each time
            byte[] iv = { 1, 2, 3, 4, 5, 6, 7, 8 };
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            // Decryption
            using (var fs = new FileStream(serializationFile, FileMode.Open, FileAccess.Read))
            {
                var cryptoStream = new CryptoStream(fs, des.CreateDecryptor(key, iv), CryptoStreamMode.Read);
                BinaryFormatter formatter = new BinaryFormatter();

                // This is where you deserialize the class
                playerlist = (PlayerInfo[])formatter.Deserialize(cryptoStream);
            }

            activeSlot = 0;
            this.count = playerlist.Length - 1;
        }

        public void savePlayerStore(string file)
        {
            string dir = @"..\..\..\..\XNA_ScreenManagerContent\playerstore\";
            string serializationFile = Path.Combine(dir, file);

            byte[] key = { 1, 2, 3, 4, 5, 6, 7, 8 }; // Where to store these keys is the tricky part, you may need to obfuscate them or get the user to input a password each time
            byte[] iv = { 1, 2, 3, 4, 5, 6, 7, 8 };
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            // Encryption
            using (var fs = new FileStream(serializationFile, FileMode.Create, FileAccess.Write))
            {
                var cryptoStream = new CryptoStream(fs, des.CreateEncryptor(key, iv), CryptoStreamMode.Write);
                BinaryFormatter formatter = new BinaryFormatter();

                // This is where you serialize the class
                formatter.Serialize(cryptoStream, playerlist);
                cryptoStream.FlushFinalBlock();
            }
        }
    }
}
