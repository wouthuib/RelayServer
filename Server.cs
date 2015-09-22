using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Linq;
using RelayServer.WorldObjects;
using RelayServer.WorldObjects.Entities;
using RelayServer.WorldObjects.Effects;
using Microsoft.Xna.Framework;
using RelayServer.Database.Accounts;
using RelayServer.Database.Players;
using RelayServer.Static;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MapleLibrary;

namespace RelayServer
{
    public class Server
    {
        //Singleton in case we need to access this object without a reference (call <Class_Name>.singleton)
        public static Server singleton;
        private Object thisLock = new Object();

        //Encryption Enabled
        public bool encryption = false;

        //Create an object of the Listener class.
        Listener listener;
        public Listener Listener
        {
            get { return listener; }
        }

        //Array of clients
        public Client[] client;

        //number of connected clients
        int connectedClients = 0;

        //Writers and readers to manipulate data
        MemoryStream readStream;
        MemoryStream writeStream;
        BinaryReader reader;
        BinaryWriter writer;

        /// <summary>
        /// Create a new Server object
        /// </summary>
        /// <param name="port">The port you want to use</param>
        public Server(int port)
        {
            //Initialize the array with a maximum of the MaxClients from the config file.
            client = new Client[Properties.Settings.Default.MaxNumberOfClients];

            //Create a new Listener object
            listener = new Listener(port);
            listener.userAdded += new ConnectionEvent(listener_userAdded);
            listener.Start();

            OutputManager.WriteLine("\t Server listening on port: {0}:{1}", 
                new string[]{listener.ipAddress.ToString(), port.ToString() });

            //Create the readers and writers.
            readStream = new MemoryStream();
            writeStream = new MemoryStream();
            reader = new BinaryReader(readStream);
            writer = new BinaryWriter(writeStream);

            //Set the singleton to the current object
            Server.singleton = this;
        }

        /// <summary>
        /// Method that is performed when a new user is added.
        /// </summary>
        /// <param name="sender">The object that sent this message</param>
        /// <param name="user">The user that needs to be added</param>
        private void listener_userAdded(object sender, Client user)
        {
            connectedClients++;

            //Send a message to every other client notifying them on a new client, if the setting is set to True
            if (Properties.Settings.Default.SendMessageToClientsWhenAUserIsAdded)
            {
                writeStream.Position = 0;

                //Write in the form {Protocol}{User_ID}{User_IP}
                writer.Write(Properties.Settings.Default.NewPlayerByteProtocol);
                writer.Write(user.id);
                writer.Write(user.IP);

                SendData(GetDataFromMemoryStream(writeStream), user);
            }

            //Set up the events
            user.DataReceived += new DataReceivedEvent(user_DataReceived);
            user.UserDisconnected += new ConnectionEvent(user_UserDisconnected);

            //Print the new player message to the server window.
            OutputManager.WriteLine(user.ToString() + "\t connected! \t Connected Clients:  " + connectedClients);

            //Add to the client array
            client[user.id] = user;
        }

        /// <summary>
        /// Method that is performed when a new user is disconnected.
        /// </summary>
        /// <param name="sender">The object that sent this message</param>
        /// <param name="user">The user that needs to be disconnected</param>
        public void user_UserDisconnected(object sender, Client user)
        {
            connectedClients--;

            //Send a message to every other client notifying them on a removed client, if the setting is set to True
            if (Properties.Settings.Default.SendMessageToClientsWhenAUserIsRemoved)
            {
                writeStream.Position = 0;

                //Write in the form {Protocol}{User_ID}{User_IP}
                writer.Write(Properties.Settings.Default.DisconnectedPlayerByteProtocol);
                writer.Write(user.id);
                writer.Write(user.IP);

                SendData(GetDataFromMemoryStream(writeStream), user);
            }

            // Save user on server
            clientfunction.saveCharacter(user);

            // Set player offline
            if (PlayerStore.Instance.playerStore.FindAll(p => p.AccountID == user.AccountID).Count > 0)
                foreach (var player in PlayerStore.Instance.playerStore.Where(p => p.AccountID == user.AccountID))
                {
                    player.Online = false;
                    SendObject(new playerData() { Name = player.Name, Action = "Remove" });
                    
                    //remove existing sprites on server
                    if (GameWorld.Instance.listEntity.FindAll(s => s.EntityName == player.Name).Count > 0)
                        foreach (var sprite in GameWorld.Instance.listEntity.Where(s => s.EntityName == player.Name))
                        {
                            sprite.KeepAliveTime = 0;
                        }
                }

            //Print the new player message to the server window.
            OutputManager.WriteLine(user.ToString() + "\t disconnected! \t Connected Clients:  " + connectedClients);

            //Clear the array's index
            client[user.id] = null;
        }

        /// <summary>
        /// Relay messages sent from one client and send them to others
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="data">The data to relay</param>
        private void user_DataReceived(Client sender, byte[] data)
        {
            writeStream.Position = 0;

            if (Properties.Settings.Default.EnableSendingIPAndIDWithEveryMessage)
            {
                //Append the id and IP of the original sender to the message, and combine the two data sets.
                writer.Write(sender.id);
                writer.Write(sender.IP);
                data = CombineData(data, writeStream);
            }

            if (data.Length > 100)
            {
                if (sender.Autenticated)
                    lock (thisLock)
                        ReadUserData(data, sender);
                else
                    lock (thisLock)
                        ReadAccountDataStream(data, sender);
            }

            //If we want the original sender to receive the same message it sent, we call a different method
            //if (Properties.Settings.Default.SendBackToOriginalClient)
            //    SendData(data);
            //else
            //    SendData(data, sender);
        }

        /// <summary>
        /// Converts a MemoryStream to a byte array
        /// </summary>
        /// <param name="ms">MemoryStream to convert</param>
        /// <returns>Byte array representation of the data</returns>
        private byte[] GetDataFromMemoryStream(MemoryStream ms)
        {
            byte[] result;

            //Async method called this, so lets lock the object to make sure other threads/async calls need to wait to use it.
            lock (ms)
            {
                int bytesWritten = (int)ms.Position;
                result = new byte[bytesWritten];

                ms.Position = 0;
                ms.Read(result, 0, bytesWritten);
            }

            return result;
        }

        /// <summary>
        /// Combines one byte array with a MemoryStream
        /// </summary>
        /// <param name="data">Original Message in byte array format</param>
        /// <param name="ms">Message to append to the original message in MemoryStream format</param>
        /// <returns>Combined data in byte array format</returns>
        private byte[] CombineData(byte[] data, MemoryStream ms)
        {
            //Get the byte array from the MemoryStream
            byte[] result = GetDataFromMemoryStream(ms);

            //Create a new array with a size that fits both arrays
            byte[] combinedData = new byte[data.Length + result.Length];

            //Add the original array at the start of the new array
            for (int i = 0; i < data.Length; i++)
            {
                combinedData[i] = data[i];
            }

            //Append the new message at the end of the new array
            for (int j = data.Length; j < data.Length + result.Length; j++)
            {
                combinedData[j] = result[j - data.Length];
            }

            //Return the combined data
            return combinedData;
        }

        /// <summary>
        /// Sends a message to every client except the source.
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <param name="sender">Client that should not receive the message</param>
        private void SendData(byte[] data, Client sender)
        {
            foreach (Client c in client)
            {
                if (c != null && c != sender)
                {
                    c.SendData(data);
                }
            }

            //Reset the writestream's position
            writeStream.Position = 0;
        }

        /// <summary>
        /// Sends data to all clients
        /// </summary>
        /// <param name="data">Data to send</param>
        private void SendData(byte[] data)
        {
            foreach (Client c in client)
            {
                if (c != null)
                    c.SendData(data);
            }

            //Reset the writestream's position
            writeStream.Position = 0;
        }

        /// <summery>
        /// Wouter's methods
        /// </summery>
        #region Wouter's Methods

        /// <summery>
        /// Send Objects to all Clients
        /// </summery>
        /// <param name="data">Data to send</param>
        public void SendObject(Object obj)
        {
            foreach (Client c in client)
            {
                if (c != null)
                    c.SendObjectStream(obj);
            }

            //Reset the writestream's position
            // writeStream.Position = 0;
        }

        /// <summery>
        /// Send Object to one Client
        /// </summery>
        /// <param name="data">Data to send</param>
        public void SendObject(Object obj, Client user)
        {
            if (user != null)
                client[user.id].SendObjectStream(obj);

            //Reset the writestream's position
            // writeStream.Position = 0;
        }

        /// <summery>
        /// Read incoming client data
        /// </summery>
        /// <param name="data">Data to read</param>
        private void ReadUserData(byte[] byteArray, Client sender)
        {
            //message has successfully been received
            //ASCIIEncoding encoder = new ASCIIEncoding();
            //System.Diagnostics.Debug.WriteLine(encoder.GetString(byteArray, 0, byteArray.Length));
            //Object obj = null;                     

            //try
            //{
            //    string xmlDoc = encoder.GetString(byteArray, 0, byteArray.Length).ToString();
            //    XDocument doc = XDocument.Parse(xmlDoc);
            //    string rootelement = doc.Root.Name.ToString();
            //    Type elementType = Type.GetType("RelayServer.ClientObjects." + rootelement);
            //    obj = DeserializeFromXml<Object>(encoder.GetString(byteArray, 0, byteArray.Length), elementType);              

            //}
            //catch(Exception exception)
            //{
            //    OutputManager.WriteLine("Error bad package from client {0} - {1}", new string[]{ sender.IP, sender.AccountID.ToString() });
            //    OutputManager.Instance.WriteLog(exception.ToString() + "\n");
            //    OutputManager.Instance.WriteLog(encoder.GetString(byteArray, 0, byteArray.Length).ToString() + "\n");
            //}

            object obj = null;

            try
            {
                //message has successfully been received
                MemoryStream ms = new MemoryStream(byteArray);
                obj = DeserializeFromStream(ms);
            }
            catch
            {
                OutputManager.WriteLine("Error - Server line 339 - invalid data received from client!");
            }

            try
            {
                if (obj is playerData)
                {
                    playerData player = (playerData)obj;
                    bool found = false;

                    foreach (var entry in PlayerStore.Instance.playerStore)
                    {
                        if (entry != null)
                        {
                            if (entry.Name == player.Name)
                            {
                                found = true; // update existing player
                                entry.Position.X = player.PositionX;
                                entry.Position.Y = player.PositionY;

                                if (player.Action == "Online")
                                {
                                    entry.Online = true;

                                    sender.CharacterID = PlayerStore.Instance.playerStore.Find(x=>x.Name == player.Name).CharacterID;
                                    GameWorld.Instance.newEntity.Add(PlayerSprite.PlayerToSprite(player));
                                }
                                else
                                {
                                    if (GameWorld.Instance.listEntity.FindAll(x => x.EntityName == player.Name).Count > 0)
                                    {
                                        PlayerSprite sprite = (PlayerSprite)GameWorld.Instance.listEntity.Find(x => x.EntityName == player.Name);
                                        sprite.fromClientToServer(player); // update server
                                    }

                                    //SendObject(player);
                                }
                            }
                        }
                    }

                    if (!found) // add new player
                    {
                        player.IP = sender.IP.ToString();
                        player.AccountID = sender.AccountID;
                        PlayerStore.Instance.addPlayer(player);

                        if (sender.MainScreenName == "charselect")
                            SendObject(PlayerStore.Instance.toPlayerData(
                                PlayerStore.Instance.playerStore.Find(x => x.Name == player.Name)), sender);
                    }

                    //OutputManager.Write(player.Name + "'s Position X:" + player.PositionX.ToString() + "\t");
                    //OutputManager.Write("Position Y:" + player.PositionY.ToString() + "\n");
                }
                else if (obj is ChatData)
                {
                    ChatData chatdata = (ChatData)obj;
                    OutputManager.Write(chatdata.Name + "'s says:" + chatdata.Text + "\n");
                    SendObject(chatdata);
                }
                else if (obj is DmgAreaData)
                {
                    DmgAreaData dmgarea = (DmgAreaData)obj;

                    GameWorld.Instance.newEffect.Add(
                        new DamageArea(
                            dmgarea.Owner,
                            new Vector2(dmgarea.PositionX, dmgarea.PositionY),
                            new Rectangle(0, 0, dmgarea.AreaWidth, dmgarea.AreaHeigth),
                            Boolean.Parse(dmgarea.Permanent),
                            dmgarea.MobHitCount,
                            dmgarea.Timer,
                            dmgarea.DmgPercent));
                }
                else if (obj is ItemData)
                {
                    ItemData item = (ItemData)obj;

                    switch (item.action)
                    {
                        case "ReqInventory":
                            clientfunction.updateInventory(sender);
                            break;
                        case "EquipItem":
                            clientfunction.EquipItem(sender, item);
                            break;
                        case "UnEquipItem":
                            clientfunction.UnEquipItem(sender, item);
                            break;
                        case "AddItem":
                            clientfunction.AddItem(sender, item);
                            break;
                    }
                }
                else if (obj is NPCData)
                {
                    NPCData npc = (NPCData)obj;

                    switch (npc.action)
                    {
                        case "OpenShop": // for testing only, "openshop" will become server based
                            clientfunction.OpenShop(sender, npc.shopID);
                            break;
                    }
                }
                else if (obj is ScreenData)
                {
                    ScreenData screen = (ScreenData)obj;

                    if (screen.MainScreenName == "charselect")
                    {
                        sender.MainScreenName = "charselect"; // save screenname in client

                        if (PlayerStore.Instance.playerStore.FindAll(x => x.AccountID == sender.AccountID).Count > 0)
                        {
                            foreach (var player in PlayerStore.Instance.playerStore.Where(x => x.AccountID == sender.AccountID))
                            {
                                SendObject(PlayerStore.Instance.toPlayerData(player), sender);
                            }
                        }
                    }
                    else if (screen.MainScreenName == "worldmap")
                    {
                        if (screen.MainScreenPhase == "loading")
                        {
                            sender.MainScreenName = "worldmap"; // save screenname in client

                            clientfunction.updateInventory(sender); // update client inventory
                            clientfunction.updateEquipment(sender); // update client equipment

                            clientfunction.updateScreen(sender, // tell client to start the worldmap
                                new ScreenData() { 
                                    MainScreenName = "worldmap",
                                    MainScreenPhase = "loading"
                                }); 
                        }
                        else if (screen.MainScreenPhase == "finish")
                        {
                            clientfunction.loadmap(sender);
                            clientfunction.updateScreen(sender, new ScreenData()
                            {
                                MainScreenName = "worldmap",
                                MainScreenPhase = "finish"
                            });
                        }
                    }
                }
            }
            catch
            {
                OutputManager.WriteLine("Error - Server line 437 - in object determination!");
            }
        }

        /// <summery>
        /// Authentication Method
        /// </summery>
        /// <param name="data">Data to read</param>
        private void ReadAccountDataXml(byte[] byteArray, Client sender)
        {
            //message has successfully been received
            ASCIIEncoding encoder = new ASCIIEncoding();
            System.Diagnostics.Debug.WriteLine(encoder.GetString(byteArray, 0, byteArray.Length));

            string xmlDoc = encoder.GetString(byteArray, 0, byteArray.Length).ToString();
            XDocument doc = XDocument.Parse(xmlDoc);
            string rootelement = doc.Root.Name.ToString();
            Type elementType = Type.GetType("RelayServer.ClientObjects." + rootelement);

            Object obj = DeserializeFromXml<Object>(encoder.GetString(byteArray, 0, byteArray.Length), elementType);

            if (obj is AccountData)
            {
                AccountData account = (AccountData)obj;

                AccountData a = new AccountData()
                {
                    Username = account.Username,
                    Password = account.Password
                };

                if (AccountStore.Instance.FindAccount(account.Username, account.Password) != 0)
                {
                    sender.AccountID = AccountStore.Instance.FindAccount(account.Username, account.Password);
                    sender.Autenticated = true;
                    a.Connected = "true";
                    SendObject(a, sender);
                }
                else
                {
                    a.Connected = "false";
                    SendObject(a, sender);
                }

                if (!sender.Autenticated)
                    OutputManager.WriteLine(account.Username + ", with IP " + sender.IP + " cannot be autenticated.");
                else
                    OutputManager.WriteLine(account.Username + ", with IP " + sender.IP + " autenticated! \t Account ID: " + sender.AccountID.ToString());
            }
        }

        /// <summery>
        /// Authentication Method
        /// </summery>
        /// <param name="data">Data to read</param>
        private void ReadAccountDataStream(byte[] byteArray, Client sender)
        {
            //message has successfully been received
            MemoryStream ms = new MemoryStream(byteArray);
            object obj = DeserializeFromStream(ms);

            if (obj is AccountData)
            {
                AccountData account = (AccountData)obj;

                AccountData a = new AccountData()
                {
                    Username = account.Username,
                    Password = account.Password
                };

                if (AccountStore.Instance.FindAccount(account.Username, account.Password) != 0)
                {
                    sender.AccountID = AccountStore.Instance.FindAccount(account.Username, account.Password);
                    sender.Autenticated = true;
                    a.Connected = "true";
                    SendObject(a, sender);
                }
                else
                {
                    a.Connected = "false";
                    SendObject(a, sender);
                }

                if (!sender.Autenticated)
                    OutputManager.WriteLine(sender.IP + "\t" + account.Username + "\t cannot be autenticated.");
                else
                    OutputManager.WriteLine(sender.IP + "\t autenticated! \t Account ID: " + sender.AccountID.ToString());
            }
        }

        /// <summery>
        /// public Deserialization Method for XML to objects
        /// </summery>
        /// <param name="data">Data to read</param>
        public static T DeserializeFromXml<T>(string xml, Type type)
        {
            T result;
            XmlSerializer ser = new XmlSerializer(type);
            using (TextReader tr = new StringReader(xml))
            {
                result = (T)ser.Deserialize(tr);
            }
            return result;
        }

        /// <summery>
        /// public Deserialization Method for Memorystream to objects
        /// </summery>
        /// <param name="data">Data to read</param>
        public static object DeserializeFromStream(MemoryStream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);

            // Allows us to manually control type-casting based on assembly/version and type name
            // formatter.Binder = new OverrideBinder();

            stream.Seek(0, SeekOrigin.Begin);
            object o = formatter.Deserialize(stream); // Unable to find assembly

            return o;
        }

        /// <summery>
        /// public Serialization Method for Memorystream to objects
        /// </summery>
        /// <param name="data">Data to read</param>
        public static MemoryStream SerializeToStream(object o)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, o);
            return stream;
        }
        #endregion
    }
}
