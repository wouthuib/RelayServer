using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace RelayServer
{
    public class Client
    {
        //Encapsulated 
        private TcpClient client;

        //Byte array that is populated when a user receives data
        private byte[] readBuffer;

        //Create the events
        public event ConnectionEvent UserDisconnected;
        public event DataReceivedEvent DataReceived;

        //The ID of this client, the constructor is only allowed to set this variable
        public readonly byte id;

        //IP of the connected client
        public string IP;

        //Is this client disconnected?
        bool connected = false;

        /// <summary>
        /// Create a new client
        /// </summary>
        /// <param name="client">TcpClient object to use</param>
        /// <param name="id">ID to give to the client</param>
        public Client(TcpClient client, byte id)
        {
            readBuffer = new byte[Properties.Settings.Default.ReadBufferSize];
            this.id = id;
            this.client = client;
            IP = client.Client.RemoteEndPoint.ToString();
            client.NoDelay = true;

            StartListening();
            connected = true;
        }

        /// <summary>
        /// Create an empty Client object
        /// </summary>
        /// <param name="ip">IP to give to the client</param>
        /// <param name="port">Port to connect</param>
        public Client(string ip, int port)
        {
            readBuffer = new byte[Properties.Settings.Default.ReadBufferSize];
            id = byte.MaxValue;
            client = new TcpClient();
            client.NoDelay = true;
            client.Connect(ip, port);

            StartListening();
            connected = true;
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
            client.GetStream().BeginRead(readBuffer, 0, Properties.Settings.Default.ReadBufferSize, StreamReceived, null);
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
                lock (client.GetStream())
                {
                    bytesRead = client.GetStream().EndRead(ar);
                }
            }

            catch (Exception e) { string error = e.ToString(); }

            //An error happened that created bad data
            if (bytesRead == 0)
            {
                Disconnect();
                Console.WriteLine("Client {0}:  {1}\n{2}", IP, "Bad data", "Disconnecting");
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
                lock (client.GetStream())
                {
                    client.GetStream().BeginWrite(b, 0, b.Length, null, null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Client {0}:  {1}", IP, e.ToString());
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
        /// String representation of the Client
        /// </summary>
        /// <returns>IP address</returns>
        public override string ToString()
        {
            return IP;
        }
    }
}
