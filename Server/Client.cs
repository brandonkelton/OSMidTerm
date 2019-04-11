using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    internal class Client
    {
        public Guid Id { get; set; }
        public Socket Socket { get; set; }
        public Thread Thread { get; set; }
    }
}
