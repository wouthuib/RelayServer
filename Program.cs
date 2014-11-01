using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RelayServer.WorldObjects;
using RelayServer.WorldObjects.Structures;
using RelayServer.Static;

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

                using (GameWorld world = new GameWorld())
                {
                    try
                    {
                        world.Run();
                    }
                    catch (Exception ee)
                    {
                        OutputManager.WriteLine("Error! {0}", new string[] { ee.ToString() });
                    }
                    finally
                    {
                        OutputManager.WriteLine("Error! GameWorld was stopped for an unknown reason!");
                    }
                }

                //while (true) { Thread.Sleep(100); }
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Display the current settings to the window
        /// </summary>
        private static void WriteSettings()
        {
            OutputManager.WriteLine("- Loading Server Settings:");
            OutputManager.WriteLine("\t SendBackToOriginalClient = " + Properties.Settings.Default.SendBackToOriginalClient);
            OutputManager.WriteLine("\t Port = " + Properties.Settings.Default.Port);
            OutputManager.WriteLine("\t ReadBufferSize = " + Properties.Settings.Default.ReadBufferSize);
            OutputManager.WriteLine("\t MaxNumberOfClients = " + Properties.Settings.Default.MaxNumberOfClients);
            OutputManager.WriteLine("\t NewPlayerByteProtocol = " + Properties.Settings.Default.NewPlayerByteProtocol);
            OutputManager.WriteLine("\t DisconnectedPlayerByteProtocol = " + Properties.Settings.Default.DisconnectedPlayerByteProtocol);
            OutputManager.WriteLine("\t SendMessageToClientsWhenAUserIsAdded = " + Properties.Settings.Default.SendMessageToClientsWhenAUserIsAdded);
            OutputManager.WriteLine("\t SendMessageToClientsWhenAUserIsRemoved = " + Properties.Settings.Default.SendMessageToClientsWhenAUserIsRemoved);
            OutputManager.WriteLine("\t EnableSendingIPAndIDWithEveryMessage = " + Properties.Settings.Default.EnableSendingIPAndIDWithEveryMessage);
        }
    }
}
