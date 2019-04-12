using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class Client
    {
        private Socket _socket;

        public async Task Connect(string hostName, int port)
        {
            var host = Dns.GetHostEntry(hostName);
            var endpoint = new IPEndPoint(host.AddressList[0], port);
            await StartConnecion(endpoint);
        }

        public async Task Connect(string hostAndPort)
        {
            int port;
            IPAddress ipAddress;
            var split = hostAndPort.Split(":");

            if (split.Length < 2 || String.IsNullOrEmpty(split[0]) || String.IsNullOrEmpty(split[1])
                || !IPAddress.TryParse(split[0], out ipAddress) || !int.TryParse(split[1], out port))
            {
                throw new ArgumentException("Invalid hostAndPort argument. Example: 127.0.0.1:12000");
            }

            var endpoint = new IPEndPoint(ipAddress, port);
            await StartConnecion(endpoint);
        }

        private async Task StartConnecion(IPEndPoint endpoint)
        {
            _socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.IP);
            await _socket.ConnectAsync(endpoint);
        }

        public async Task<string> Receive()
        {
            var buffer = new Memory<byte>();
            var result = await _socket.ReceiveAsync(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer.ToArray());
            return response;
        }

        public async Task Send(string text)
        {
            var textBytes = Encoding.UTF8.GetBytes(text);
            var buffer = new ReadOnlyMemory<byte>(textBytes);
            var result = await _socket.SendAsync(buffer, SocketFlags.None);
        }

        public void Kill()
        {
            _socket.Close();
            _socket.Disconnect(true);
            _socket.Dispose();
        }
    }
}
