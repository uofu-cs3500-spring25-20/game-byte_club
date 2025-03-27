// <copyright file="ChatServer.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>

using CS3500.Networking;
using System.ComponentModel.Design;
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
            // initialize client variables
            bool IsInDictionary = false;
            string clientName = "";
            string message = "";

            while (true)
            {
                // check if there is a connection, if there's not, handle it in catch block
                if (connection.IsConnected)
                {
                    // check if the client is in the dictionary, if not, add them
                    while (!IsInDictionary)
                    {
                        // prompt the client for their name
                        connection.Send("Enter your name");
                        clientName = connection.ReadLine();
                        // if the are no other clients with that name in the server, add them to the dictionary
                        if (!clients.ContainsValue(clientName))
                        {
                            lock (clients)
                            { // CHECK THAT THIS IS THE RIGHT LOCK ---------------------
                                clients.Add(connection, clientName);
                                IsInDictionary = true;
                            }

                            // send a message to all clients that the new client has joined
                            message = clientName + " has joined the chat";
                            foreach (NetworkConnection c in clients.Keys)
                            {
                                c.Send(message);
                            }
                        }
                        // the name the clent chose is already taken, prompt them to choose a different name
                        else connection.Send("Name already taken, please enter a different name: ");
                    }

                    // read the message from the client and send it to all other clients
                    message = connection.ReadLine();
                    foreach (NetworkConnection c in clients.Keys)
                    {
                        c.Send(clientName + ": " + message);
                    }
                }


                else throw new InvalidOperationException();
            }
        }
        catch (Exception)
        {
            string name = clients[connection];
            // do anything necessary to handle a disconnected client in here
            lock (clients)
            { // CHECK THAT THIS IS THE RIGHT LOCK ---------------------
                clients.Remove(connection);
                clientCount--;
            }
            //remove the client from the dictionary and send a message to all other clients that they have disconnected
            foreach (NetworkConnection c in clients.Keys)
            {
                c.Send(name + " has disconnected.");
            }
        }
    }
}