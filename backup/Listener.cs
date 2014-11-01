using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using RelayServer.WorldObjects.Structures;
using RelayServer.Static;
using System.Threading;

namespace RelayServer
{
    
    /// <summary>
    /// Server socket Example, The listener process
    /// </summary>
    /// <param name="portNr">http://msdn.microsoft.com/en-us/library/fx6588te(v=vs.110).aspx</param>
    public class Listener
    {
        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        private Socket listener;
        public bool Active;

        //send an even once we receive a user
        public event ConnectionEvent userAdded;

        //a variable to keep track of how many users we've added
        private bool[] usedUserID;

        /// <summary>
        /// Create a new Listener object
        /// </summary>
        /// <param name="portNr">Port to use</param>
        public Listener(int portNr)
        {
            //Create an array to hold the used IDs
            usedUserID = new bool[Properties.Settings.Default.MaxNumberOfClients];

            //Create the internal TcpListener
            //listener = new TcpListener(IPAddress.Any, portNr);
            listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp );
        }

        /// <summary>
        /// Starts a new session of listening for messages.
        /// </summary>
        public void Start()
        {
            //listener.Start();
            StartListening();
            Active = true;
        }

        /// <summary>
        /// Stops listening for messages.
        /// </summary>
        public void Stop()
        {
            //listener.Stop();
            Active = false;
        }

        /// <summary>
        /// Used for allowing new users to connect
        /// </summary>
        private void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 1490);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    listener.BeginAccept(
                        new AsyncCallback(AcceptClient),
                        listener);

                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Called when a client connects to the server
        /// </summary>
        /// <param name="ar">Status of the Async method</param>
        private void AcceptClient(IAsyncResult ar)
        {
            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket client = listener.EndAccept(ar);

            // Signal the main thread to continue.
            allDone.Set();

            //id is originally -1 which means a user cannot connect
            int id = -1;
            for (byte i = 0; i < usedUserID.Length; i++)
            {
                if (usedUserID[i] == false)
                {
                    id = i;
                    break;
                }
            }

            //If the id is still -1, the client what wants to connect cannot (probably because we have reached the maximum number of clients
            if (id == -1)
            {
                OutputManager.WriteLine("Client " + client.RemoteEndPoint.ToString() + " cannot connect. ");
                return;
            }

            //ID is valid, so create a new Client object with the server ID and IP
            usedUserID[id] = true;
            Client newClient = new Client(client, (byte)id);

            //We are now connected, so we need to set up the User Disconnected event for this user.
            newClient.UserDisconnected += new ConnectionEvent(client_UserDisconnected);

            //We are now connected, so call all delegates of the UserAdded event.
            if (userAdded != null)
                userAdded(this, newClient);

            //Begin listening for new clients
            //StartListening();
        }

        /// <summary>
        /// User disconnects from the server
        /// </summary>
        /// <param name="sender">Original object that called this method</param>
        /// <param name="user">Client to disconnect</param>
        void client_UserDisconnected(object sender, Client user)
        {
            usedUserID[user.id] = false;
        }

    }
}
