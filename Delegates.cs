using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelayServer
{
    public delegate void ConnectionEvent(object sender, Client user);
    public delegate void DataReceivedEvent(Client sender, byte[] data);
}
