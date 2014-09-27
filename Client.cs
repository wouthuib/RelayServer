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
using RelayServer.Database.Accounts;
using System.Security.Cryptography;

namespace RelayServer
{
    public class Client : ClientFunctions
    {
        //Encapsulated 
        private TcpClient client;

        //Byte array that is populated when a user receives data
        private byte[] readBuffer;

        // client stream
        private NetworkStream networkstream;

        //Create the events
        public event ConnectionEvent UserDisconnected;
        public event DataReceivedEvent DataReceived;

        //The ID of this client, the constructor is only allowed to set this variable
        public readonly byte id;

        //IP of the connected client
        public string IP;

        /// <summary>
        /// Create a new client
        /// </summary>
        /// <param name="client">TcpClient object to use</param>
        /// <param name="id">ID to give to the client</param>
        public Client(TcpClient client, byte id)
            : base()
        {
            readBuffer = new byte[1024];
            this.id = id;
            this.client = client;
            IP = client.Client.RemoteEndPoint.ToString();
            //client.NoDelay = true;

            this.user = this;
            networkstream = client.GetStream();

            StartListening();
        }

        /// <summary>
        /// Create an empty Client object
        /// </summary>
        /// <param name="ip">IP to give to the client</param>
        /// <param name="port">Port to connect</param>
        public Client(string ip, int port)
            : base()
        {
            readBuffer = new byte[Properties.Settings.Default.ReadBufferSize];
            id = byte.MaxValue;
            client = new TcpClient();
            client.NoDelay = true;
            client.Connect(ip, port);

            this.user = this;
            StartListening();
        }

        /// <summary>
        /// Disconnect the client from the server
        /// </summary>
        public void Disconnect()
        {
            if (connected)
            {
                connected = false;

                client.Close();

                //Call all delegates
                if (UserDisconnected != null)
                    UserDisconnected(this, this);
            }
        }

        /// <summary>
        /// Start listening for new data
        /// </summary>
        private void StartListening()
        {
            networkstream.BeginRead(readBuffer, 0, StateObject.BufferSize, StreamReceived, null);
        }

        /// <summary>
        /// Data was received
        /// </summary>
        /// <param name="ar">Async status</param>
        private void StreamReceived(IAsyncResult ar)
        {
            int bytesRead = 0;
            try
            {
                lock (networkstream)
                {
                    bytesRead = networkstream.EndRead(ar);
                }
            }

            catch (Exception e) { string error = e.ToString(); }

            //An error happened that created bad data
            if (bytesRead == 0)
            {
                Disconnect();
                OutputManager.WriteLine("Error! Client {0}: {1} {2}", new string[]{IP, "link broken!", "Disconnecting"});
                return;
            }

            //Create the byte array with the number of bytes read
            byte[] data = new byte[bytesRead];

            //Populate the array
            for (int i = 0; i < bytesRead; i++)
                data[i] = readBuffer[i];

            //Listen for new data
            StartListening();

            //Call all delegates
            if (DataReceived != null)
                DataReceived(this, data);
        }

        /// <summary>
        /// Code to actually send the data to the client
        /// </summary>
        /// <param name="b">Data to send</param>
        public void SendData(byte[] b)
        {
            //Try to send the data.  If an exception is thrown, disconnect the client
            try
            {
                lock (networkstream)
                {
                    if (networkstream.CanWrite)
                        networkstream.BeginWrite(b, 0, b.Length, null, null);
                }
            }
            catch (Exception e)
            {
                OutputManager.WriteLine("Error! Sending data to Client {0}:  {1}", new string[] { IP, e.ToString() });
            }
        }

        /// <summary>
        /// Code to send data in a MemoryStream format
        /// </summary>
        /// <param name="ms">The data to send</param>
        public void SendMemoryStream(System.IO.MemoryStream ms)
        {
            lock (ms)
            {
                int bytesWritten = (int)ms.Position;
                byte[] result = new byte[bytesWritten];

                ms.Position = 0;
                ms.Read(result, 0, bytesWritten);
                SendData(result);
            }
        }

        /// <summary>
        /// By Wouter Code to actually send the Object to the client
        /// </summary>
        /// <param name="b">Data to send</param>
        public void SendObject(Object obj)
        {
            // Try to send the data.  If an exception is thrown, disconnect the client
            try
            {
                lock (networkstream)
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Object));
                    bool getobject = false;

                    if (obj is MonsterData)
                    {
                        if (MainScreenName == "worldmap")
                        {
                            xmlSerializer = new XmlSerializer(typeof(MonsterData));
                            getobject = true;
                        }
                    }
                    else if (obj is playerData)
                    {
                        xmlSerializer = new XmlSerializer(typeof(playerData));
                        getobject = true;
                    }
                    else if (obj is AccountData)
                    {
                        xmlSerializer = new XmlSerializer(typeof(AccountData));
                        getobject = true;
                    }
                    else if (obj is EffectData)
                    {
                        xmlSerializer = new XmlSerializer(typeof(EffectData));
                        getobject = true;
                    }
                    else if (obj is ChatData)
                    {
                        xmlSerializer = new XmlSerializer(typeof(ChatData));
                        getobject = true;
                    }
                    else if (obj is PlayerInfo)
                    {
                        xmlSerializer = new XmlSerializer(typeof(PlayerInfo));
                        //obj = PlayerStore.Instance.toPlayerData((PlayerInfo)obj);
                        getobject = false;
                    }
                    else if (obj is List<PlayerInfo>)
                    {
                        xmlSerializer = new XmlSerializer(typeof(List<PlayerInfo>));
                        getobject = false;
                    }

                    if (getobject)
                    {
                        StringWriter stringwriter = new StringWriter();
                        XmlWriter xmlwriter = XmlWriter.Create(stringwriter);
                        xmlSerializer.Serialize(xmlwriter, obj);

                        // new send encrypted message (in process) !!!
                        //var bytes = Encoding.UTF8.GetBytes(stringwriter.ToString());
                        //var base64 = Convert.ToBase64String(bytes);

                        //string encrypted = RijndaelSimple.Encrypt(
                        //    stringwriter.ToString(),
                        //    "Pas5pr@se",
                        //    "s@1tValue",
                        //    "SHA1",
                        //    2,
                        //    "@1B2c3D4e5F6g7H8",
                        //    256);

                        byte[] myWriteBuffer = null;

                        if (Server.singleton.encryption)
                            myWriteBuffer = Encoding.ASCII.GetBytes(encryptString(stringwriter.ToString(), "Assesjode"));
                        else
                            myWriteBuffer = Encoding.ASCII.GetBytes(stringwriter.ToString());
                        
                        try
                        {
                            if (networkstream.CanWrite)
                                networkstream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                            // flush streams
                            networkstream.Flush();
                        }
                        catch
                        {
                            OutputManager.WriteLine("Error! Unable to send data to client {0}, {1}, {2}",
                                new string[] { this.IP, this.AccountID.ToString(), 
                                    AccountStore.Instance.account_list.Find(x=>x.AccountID == this.AccountID).Username});
                        }

                        // start stringwriter, xmlwriter and serialize
                        StringWriter sww = new StringWriter();
                        XmlWriter writer = XmlWriter.Create(sww);
                        xmlSerializer.Serialize(writer, obj);                        

                        //SendData(StringToBytes(encrypted));
                    }

                }
            }
            catch (Exception e)
            {
                OutputManager.WriteLine("Error! Sending data to Client {0}: \n\n {1}", new string[] { IP, e.ToString() });
            }
        }

        public byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        static byte[] StringToBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public System.IO.MemoryStream ObjectToMemoryStream(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms;
        }

        /// <summary>
        /// String representation of the Client
        /// </summary>
        /// <returns>IP address</returns>
        public override string ToString()
        {
            return IP;
        }

        // create and initialize a crypto algorithm
        private static SymmetricAlgorithm getAlgorithm(string password)
        {
            SymmetricAlgorithm algorithm = Rijndael.Create();
            Rfc2898DeriveBytes rdb = new Rfc2898DeriveBytes(
                password, new byte[] {
            0x53,0x6f,0x64,0x69,0x75,0x6d,0x20,             // salty goodness
            0x43,0x68,0x6c,0x6f,0x72,0x69,0x64,0x65
        }
            );
            algorithm.Padding = PaddingMode.ISO10126;
            algorithm.Key = rdb.GetBytes(32);
            algorithm.IV = rdb.GetBytes(16);
            return algorithm;
        }

        public static string encryptString(string clearText, string password)
        {
            SymmetricAlgorithm algorithm = getAlgorithm(password);
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, algorithm.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearBytes, 0, clearBytes.Length);
            cs.Close();
            return Convert.ToBase64String(ms.ToArray());
        }

        public static string decryptString(string cipherText, string password)
        {
            SymmetricAlgorithm algorithm = getAlgorithm(password);
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, algorithm.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherBytes, 0, cipherBytes.Length);
            cs.Close();
            return System.Text.Encoding.Unicode.GetString(ms.ToArray());
        }
    }

    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

}
