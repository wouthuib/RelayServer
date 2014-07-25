using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace RelayServer
{
    public class Listener
    {
        private TcpListener listener;

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
            listener = new TcpListener(IPAddress.Any, portNr);
        }

        /// <summary>
        /// Starts a new session of listening for messages.
        /// </summary>
        public void Start()
        {
            listener.Start();
            ListenForNewClient();
        }

        /// <summary>
        /// Stops listening for messages.
        /// </summary>
        public void Stop()
        {
            listener.Stop();
        }

        /// <summary>
        /// Used for allowing new users to connect
        /// </summary>
        private void ListenForNewClient()
        {
            listener.BeginAcceptTcpClient(AcceptClient, null);
        }

        /// <summary>
        /// Called when a client connects to the server
        /// </summary>
        /// <param name="ar">Status of the Async method</param>
        private void AcceptClient(IAsyncResult ar)
        {
            //We need to end the Async method of accepting new clients
            TcpClient client = listener.EndAcceptTcpClient(ar);

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
                Console.WriteLine("Client " + client.Client.RemoteEndPoint.ToString() + " cannot connect. ");
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
            ListenForNewClient();
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
