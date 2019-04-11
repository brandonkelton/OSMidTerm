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
        private ConcurrentDictionary<Guid, Client> clients = new ConcurrentDictionary<Guid, Client>(10, 100);
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

            clients.Values.ToList().ForEach(client =>
            {
                if (client.Socket.Connected)
                {
                    KillClient(client);
                }
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

                    Client client;
                    
                    try
                    {
                        client = CreateClient();
                    }
                    catch (InvalidOperationException e)
                    {
                        var messageByteString = Encoding.UTF8.GetBytes("Could not accept client. Please try again later.");
                        var messageBuffer = new ReadOnlyMemory<byte>(messageByteString);
                        var result = await acceptedSocket.SendAsync(messageBuffer, SocketFlags.None);
                        acceptedSocket.Close();
                        acceptedSocket.Disconnect(true);
                        acceptedSocket.Dispose();

                        continue;
                    }

                    var thread = new Thread(new ThreadStart(async () => await ProcessSocket(client)));
                    client.Socket = acceptedSocket;
                    client.Thread = thread;
                    thread.Start();
                }

                socket.Close(10000);
            }

            listenerResetEvent.Set();
        }

        private async Task ProcessSocket(Client client)
        {
            var isClientActive = true;
            var tempIterations = 0;

            while (isClientActive)
            {
                if (!client.Socket.Connected)
                {
                    isClientActive = false;
                }

                var buffer = new Memory<byte>();
                var result = await client.Socket.ReceiveAsync(buffer, SocketFlags.None);
                Console.WriteLine("ClientResult: " + result);

                var receivedMessage = buffer.ToString();
                Console.WriteLine("ReceivedMessage: " + receivedMessage);

                if (tempIterations == 3)
                {
                    KillClient(client);

                }
            }
        }

        private Client CreateClient()
        {
            var client = new Client
            {
                Id = Guid.NewGuid()
            };

            var maxAttempts = 60;
            var attempts = 0;
            while (!clients.TryAdd(client.Id, client))
            {
                if (attempts >= maxAttempts)
                {
                    throw new InvalidOperationException("Error attempting to add client to ConcurrentBag");
                }

                attempts++;
                Thread.Sleep(1000);
            }

            return client;
        }

        private static void KillClient(Client client)
        {
            if (client.Socket.Connected)
            {
                client.Socket.Close();
                client.Socket.Disconnect(true);
            }

            client.Socket.Dispose();
        }
    }
}
