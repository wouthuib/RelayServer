using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RelayServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //Display the current settings to the window
            WriteSettings();

            //Try to start a new server using the default port in the config file.
            try
            {
                Server server = new Server(Properties.Settings.Default.Port);

                while (true) { Thread.Sleep(1000); }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.ReadKey();
        }

        /// <summary>
        /// Display the current settings to the window
        /// </summary>
        private static void WriteSettings()
        {
            Console.WriteLine("Server Settings: ");
            Console.WriteLine("SendBackToOriginalClient = " + Properties.Settings.Default.SendBackToOriginalClient);
            Console.WriteLine("Port = " + Properties.Settings.Default.Port);
            Console.WriteLine("ReadBufferSize = " + Properties.Settings.Default.ReadBufferSize);
            Console.WriteLine("MaxNumberOfClients = " + Properties.Settings.Default.MaxNumberOfClients);
            Console.WriteLine("NewPlayerByteProtocol = " + Properties.Settings.Default.NewPlayerByteProtocol);
            Console.WriteLine("DisconnectedPlayerByteProtocol = " + Properties.Settings.Default.DisconnectedPlayerByteProtocol);
            Console.WriteLine("SendMessageToClientsWhenAUserIsAdded = " + Properties.Settings.Default.SendMessageToClientsWhenAUserIsAdded);
            Console.WriteLine("SendMessageToClientsWhenAUserIsRemoved = " + Properties.Settings.Default.SendMessageToClientsWhenAUserIsRemoved);
            Console.WriteLine("EnableSendingIPAndIDWithEveryMessage = " + Properties.Settings.Default.EnableSendingIPAndIDWithEveryMessage);
            Console.WriteLine("\nIf these settings are incorrect, please close the server and open the config file");
        }
    }
}
