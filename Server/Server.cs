using ClientServer.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Server
    {
        private ManualResetEvent listenerResetEvent = new ManualResetEvent(false);
        private ConcurrentBag<Client> clients = new ConcurrentBag<Client>();
        private ConcurrentBag<Thread> socketThreads = new ConcurrentBag<Thread>();
        private bool CanServerListen = true;

        public void StopServer()
        {
            CanServerListen = false;

            listenerResetEvent.WaitOne();

            socketThreads.ToList().ForEach(thread =>
            {
                thread.Join();
            });

            clients.ToList().ForEach(client =>
            {
                if (client.Socket.Connected)
                {
                    client.Socket.Disconnect(true);
                }
                client.Socket.Dispose();
            });
        }

        async Task Listen()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var endpoint = new IPEndPoint(host.AddressList[0], IPEndPoint.MaxPort);

            using (var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Listen(100);

                while (CanServerListen)
                {
                    var acceptedSocket = await socket.AcceptAsync();
                    var processSocketThread = new Thread(new ThreadStart(async () => await ProcessSocket(acceptedSocket)));
                    processSocketThread.Start();
                    socketThreads.Add(processSocketThread);
                }

                socket.Close(1000);
            }

            listenerResetEvent.Set();
        }

        private async Task ProcessSocket(Socket socket)
        {
            var client = new Client { Socket = socket };
            clients.Add(client);

            var isClientActive = true;

            while (isClientActive)
            {
                if (!socket.Connected)
                {
                    isClientActive = false;
                }

                var buffer = new Memory<byte>();
                var result = await socket.ReceiveAsync(buffer, SocketFlags.None);
                Console.WriteLine("ClientResult: " + result);

                var receivedMessage = buffer.ToString();
                Console.WriteLine("ReceivedMessage: " + receivedMessage);


            }
        }

        private void DisconnectSocket(Socket socket)
        {
            if (!socket.Connected)
            {
                socket.Disconnect(true);
            }
        }
    }
}
