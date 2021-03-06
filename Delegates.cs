﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace RelayServer
{
    public delegate void DisconnectEvent(object sender, Client user);
    public delegate void ConnectionEvent(object sender, Client user);
    public delegate void DataReceivedEvent(Client sender, byte[] data);
}
