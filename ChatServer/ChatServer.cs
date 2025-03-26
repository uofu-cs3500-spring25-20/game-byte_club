// <copyright file="ChatServer.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>

using CS3500.Networking;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;

namespace CS3500.Chatting;

/// <summary>
///   A simple ChatServer that handles clients separately and replies with a static message.
/// </summary>
public partial class ChatServer
{
    private static Dictionary<NetworkConnection, string> clients = new Dictionary<NetworkConnection, string>(); //--------------------------------
    private static int clientCount = 0; // MADE STATIC ---------------

    /// <summary>
    ///   The main program.
    /// </summary>
    /// <param name="args"> ignored. </param>
    /// <returns> A Task. Not really used. </returns>
    private static void Main(string[] args)
    {
        Server.StartServer(HandleConnect, 11_000);
        Console.Read(); // don't stop the program.
    }


    /// <summary>
    ///   <pre>
    ///     When a new connection is established, enter a loop that receives from and
    ///     replies to a client.
    ///   </pre>
    /// </summary>
    ///
    private static void HandleConnect(NetworkConnection connection)
    { 
        // handle all messages until disconnect.
        try
        {
            while (true)
            {
                bool IsInDictionary = false;
                string clientName;
                string message;

                if (!connection.IsConnected)
                {
                    throw new InvalidOperationException();
                }

                while (!IsInDictionary)
                {
                    connection.Send("Enter your name: ");
                    clientName = connection.ReadLine();
                    if(!clients.ContainsValue(clientName))
                    {
                        lock (clients)
                        { // CHECK THAT THIS IS THE RIGHT LOCK ---------------------
                            clients.Add(connection, clientName);
                            IsInDictionary = true;
                        }
                        message = clientName + " has joined the chat";
                        foreach (NetworkConnection c in clients.Keys)
                        {
                            c.Send(message);
                        }
                    }
                    else connection.Send("Name already taken, please enter a different name: ");
                }

                message = connection.ReadLine();
                foreach (NetworkConnection c in clients.Keys)
                {
                    c.Send(message);
                }
            }
        }
        catch (InvalidOperationException e)
        {
            // do anything necessary to handle a disconnected client in here
            string name = clients[connection];
            foreach (NetworkConnection c in clients.Keys)
            {
                c.Send(name + " has disconnected.");
            }

            lock (clients)
            { // CHECK THAT THIS IS THE RIGHT LOCK ---------------------
                clients.Remove(connection);
                clientCount--;
            }
        }
    }


}