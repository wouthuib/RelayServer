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
using RelayServer.Database.Accounts;
using System.Security.Cryptography;
using RelayServer.Static;
using System.Runtime.Serialization;
using MapleLibrary;
using System.Threading;
using System.Net;

namespace RelayServer
{
    /// <summary>
    /// Server socket Example, The client send and receive processes
    /// </summary>
    /// <param name="portNr">http://msdn.microsoft.com/en-us/library/fx6588te(v=vs.110).aspx</param>
    public class Client : ClientFunctions
    {
        Socket client;

        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        //Byte array that is populated when a user receives data
        private byte[] readBuffer = new byte[StateObject.BufferSize];

        private object lockstream = new Object();
        private List<byte[]> objectList = new List<byte[]>();
        private bool Sending = false;

        public bool Connected = false;
        public bool encryption = false;

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
        public Client(Socket client, byte id)
            : base()
        {
            readBuffer = new byte[40000];
            this.id = id;
            this.client = client;
            //IP = client.Client.RemoteEndPoint.ToString();
            IP = client.RemoteEndPoint.ToString();

            //client.NoDelay = true;

            this.user = this;
            //networkstream = client.GetStream();

            // start send loop
            Thread sendloop = new Thread(new ThreadStart(this.SendingLoop));
            sendloop.Start();

            // start client listener
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
            client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
            client.NoDelay = true;
                        
            // Connect to the remote endpoint.
            client.BeginConnect(client.RemoteEndPoint,
                new AsyncCallback(ConnectCallback), client);
            connectDone.WaitOne();

            // start send loop
            Thread sendloop = new Thread(new ThreadStart(this.SendingLoop));
            sendloop.Start();

            // start client listener
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
            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = client;

            // Begin receiving the data from the remote device.
            client.BeginReceive(readBuffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
        }

        /// <summary>
        /// Data was received
        /// </summary>
        /// <param name="ar">Async status</param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                //An error happened that created bad data
                if (bytesRead == 0)
                {
                    Disconnect();
                    OutputManager.WriteLine("Error - Client {0}: {1} {2}", new string[] { IP, "link broken!", "Disconnecting" });
                    return;
                }

                //Create the byte array with the number of bytes read
                byte[] data = new byte[bytesRead];

                //Populate the array
                for (int i = 0; i < bytesRead; i++)
                    data[i] = readBuffer[i];

                // start new listener
                StartListening();

                //Call all delegates
                if (DataReceived != null)
                    DataReceived(this, data);
            }                
            catch (SocketException ex)
            {
                this.SocketErrorHandler(ex.ErrorCode);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Code to actually send the data to the client
        /// </summary>
        /// <param name="b">Data to send</param>
        public void SendData(byte[] byteData)
        {
            //Try to send the data.  If an exception is thrown, disconnect the client
            try
            {
                lock (lockstream)
                {
                    if (client.Connected)
                    {
                        // Begin sending the data to the remote device.
                        client.BeginSend(byteData, 0, byteData.Length, 0,
                            new AsyncCallback(SendCallback), client);
                    }
                }
            }
            catch (SocketException ex)
            {
                SocketErrorHandler(ex.ErrorCode);
            }
            catch (Exception e)
            {
                OutputManager.WriteLine("Error! Sending data to Client {0}:  {1}", new string[] { IP, e.ToString() });
            }
        }

        /// <summary>
        /// Sending Loop which processes the bytestream List
        /// </summary>
        /// <param name="b">ByteStream List to Send</param>
        public void SendingLoop()
        {
            while (true)
            {
                if (!Sending)
                {
                    if (objectList.Count > 0)
                    {
                        lock (lockstream)
                        {
                            Sending = true;
                            byte[] bytestream = new byte[objectList[0].Length];
                            bytestream = objectList[0];         //pick oldest from list
                            objectList.Remove(bytestream);      //remove from list
                            SendData(bytestream);
                        }
                    }
                }
                Thread.Sleep(10);
            }
        }
        
        /// <summary>
        /// Code to actually send the data to the client
        /// </summary>
        /// <param name="b">Data to send</param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Code to actually send the data to the client
        /// </summary>
        /// <param name="b">Confirm Send Data</param>
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();

                //Sending Done
                Sending = false;
            }
            catch (SocketException ex)
            {
                this.SocketErrorHandler(ex.ErrorCode);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
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
        /// Code to display Socket errors in Output manager
        /// </summary>
        /// <param name="ms">Display Socket error code</param>
        private void SocketErrorHandler(int errorcode)
        {
            switch (errorcode)
            {
                case 10054:     // connection was forecely closed
                    Disconnect();
                    break;
                case 10060:     // connection timeout
                    OutputManager.WriteLine("Error - connection timeout - client {0}",
                    new string[] { IP });
                    break;
                default:        // other socket exceptions
                    OutputManager.WriteLine("Error - client {0}: socket exception code {1} ",
                    new string[] { IP, errorcode.ToString() });
                    break;
            }
        }

        /// <summary>
        /// By Wouter Code to actually send the Object to the client
        /// </summary>
        /// <param name="b">Data to send</param>
        public void SendObjectXml(Object obj)
        {
            // Try to send the data.  If an exception is thrown, disconnect the client
            try
            {
                lock (lockstream)
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
                        string encrypted = RijndaelSimple.Encrypt(
                            stringwriter.ToString(),
                            "Pas5pr@se",
                            "s@1tValue",
                            "SHA1",
                            2,
                            "@1B2c3D4e5F6g7H8",
                            256);

                        byte[] myWriteBuffer = null;

                        if (Server.singleton.encryption)
                            myWriteBuffer = Encoding.ASCII.GetBytes(encrypted);
                        else
                            myWriteBuffer = Encoding.ASCII.GetBytes(stringwriter.ToString());
                        
                        try
                        {
                            //if (networkstream.CanWrite)
                            //    networkstream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                            //// flush streams
                            //networkstream.Flush();
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

        /// <summary>
        /// Code to send memory stream send bytestream list
        /// </summary>
        /// <param name="b">Object to BytestreamList</param>
        public void SendObjectStream(Object obj)
        {
            IFormatter formatter = new BinaryFormatter();

            // Try to send the data.  If an exception is thrown, disconnect the client
            try
            {
                lock (lockstream)
                {
                    bool getobject = false;

                    if (obj is MonsterData)
                    {
                        if (MainScreenName == "worldmap")
                            getobject = true;
                    }
                    else if (obj is playerData)
                        getobject = true;
                    else if (obj is AccountData)
                        getobject = true;
                    else if (obj is EffectData)
                        getobject = true;
                    else if (obj is ChatData)
                        getobject = true;
                    else if (obj is ItemData)
                        getobject = true;
                    else if (obj is HudData)
                        getobject = true;
                    else if (obj is PlayerInfo)
                        getobject = false;
                    else if (obj is List<PlayerInfo>)
                        getobject = false;

                    if (getobject)
                    {                        
                        try
                        {
                            lock (lockstream)
                            {
                                objectList.Add(SerializeToStream(obj).ToArray());
                            }
                        }
                        catch
                        {
                            OutputManager.WriteLine("Error! Unable to send data to client {0}, {1}, {2}",
                                new string[] { this.IP, this.AccountID.ToString(), 
                                    AccountStore.Instance.account_list.Find(x=>x.AccountID == this.AccountID).Username});
                        }
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

        // memory stream serializations
        public static MemoryStream SerializeToStream(object o)
        {
            using (MemoryStream stream = new MemoryStream(new byte[2048]))
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, o);
                return stream;
            }
        }
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

        /// <summary>
        /// String representation of the Client
        /// </summary>
        /// <returns>IP address</returns>
        public override string ToString()
        {
            return IP;
        }
    }

    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 40000;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

}
