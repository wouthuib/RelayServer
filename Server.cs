using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using RelayServer.ClientObjects;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace RelayServer
{
    public class Server
    {
        //Singleton in case we need to access this object without a reference (call <Class_Name>.singleton)
        public static Server singleton;

        //Create an object of the Listener class.
        Listener listener;
        public Listener Listener
        {
            get { return listener; }
        }

        //Array of clients
        Client[] client;

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
            Console.WriteLine(user.ToString() + " connected\tConnected Clients:  " + connectedClients + "\n");

            //Add to the client array
            client[user.id] = user;
        }

        /// <summary>
        /// Method that is performed when a new user is disconnected.
        /// </summary>
        /// <param name="sender">The object that sent this message</param>
        /// <param name="user">The user that needs to be disconnected</param>
        private void user_UserDisconnected(object sender, Client user)
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

            //Print the removed player message to the server window.
            Console.WriteLine(user.ToString() + " disconnected\tConnected Clients:  " + connectedClients + "\n");

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

            ReadUserData(data);

            //If we want the original sender to receive the same message it sent, we call a different method
            if (Properties.Settings.Default.SendBackToOriginalClient)
            {                
                SendData(data);
            }
            else
            {
                SendData(data, sender);
            }
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
                if(c != null)
                    c.SendData(data);
            }

            //Reset the writestream's position
            writeStream.Position = 0;
        }
        
        // Wouter's methods

        private void ReadUserData(byte[] byteArray)
        {
            //message has successfully been received
            ASCIIEncoding encoder = new ASCIIEncoding();
            System.Diagnostics.Debug.WriteLine(encoder.GetString(byteArray, 0, byteArray.Length));
            
            string xmlDoc = encoder.GetString(byteArray, 0, byteArray.Length).ToString();
            XDocument doc = XDocument.Parse(xmlDoc);
            string rootelement = doc.Root.Name.ToString();
            Type elementType = Type.GetType("RelayServer.ClientObjects." + rootelement);

            Object obj = DeserializeFromXml<Object>(encoder.GetString(byteArray, 0, byteArray.Length), elementType);

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
                            entry.PositionX = player.PositionX;
                            entry.PositionY = player.PositionY;
                        }
                    }
                }

                if (!found) // add new player
                    PlayerStore.Instance.addPlayer(player);

                Console.Write(player.Name + "'s Position X:" + player.PositionX.ToString() + "\t");
                Console.Write("Position Y:" + player.PositionY.ToString() + "\n");
            }
            else if (obj is ChatData)
            {
                ChatData chatdata = (ChatData)obj;
                Console.Write(chatdata.Name + "'s says:" + chatdata.Text + "\n");
            }
        }

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
    }
}
