using ClientServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Server
{
    internal static class CommandHandler
    {
        public static void Process(string command, Client client)
        {
            var splitCommand = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            switch (splitCommand[0].ToLower())
            {
                case CommandTypes.HELLO:
                    client.Message = "Hello " + client.Endpoint.Address + ":" + client.Endpoint.Port;
                    break;
                case CommandTypes.REFLECT:
                    client.Message = String.Join(" ", splitCommand.Skip(1));
                    break;
                case CommandTypes.REPEAT:
                    // Maintain last client message; it will be resent
                    break;
                default:
                    client.Message = "Invalid Command: " + command;
                    break;
            }
        }
    }
}
