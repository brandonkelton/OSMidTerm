using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    internal class Client
    {
        public Guid Id { get; set; }
        public IPEndPoint Endpoint { get; set; }
        public Socket Socket { get; set; }
        public Thread Thread { get; set; }
        public bool IsActive { get; set; }
        public string Message { get; set; }
    }
}
