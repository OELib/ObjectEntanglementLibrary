using OELib.PokingConnection;
using SimpleTestApp.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleTestApp.Server
{
    public class SimpleServer
    {
        private PokingServer _server;

        /// <summary>
        /// Constructor for the SimpleServer. Starts listening for client connections on the specified port
        /// </summary>
        /// <param name="port">Port to listen for connections on.</param>
        public SimpleServer(int port)
        {
            // Create a new server listening on the specified port. Use this object as a reacting object.
            // The clients will be able to invoke methods in this instance.
            _server = new PokingServer(port, this);
            _server.Start();

            _server.ClientConnected += (sender, client) =>
            {
                Console.WriteLine("A client connected");
            };
        }

        /// <summary>
        /// Sends a chat message to all connected clients
        /// </summary>
        /// <param name="chatStringToSend">Message to send</param>
        internal void SendChatTextToAllClients(string chatStringToSend)
        {
            _server.Connections.ForEach((client) =>
            {
                client.Reactor.CallRemoteMethod("HandleChatString", chatStringToSend);
            });
        }

        /// <summary>
        /// Gets a test object
        /// </summary>
        /// <returns>A new instance of the TestObject class</returns>
        public TestObject GetTestObject()
        {
            Console.WriteLine("A client requested a TestObject");

            return new TestObject() { TestString = "Test string from server" };
        }

        /// <summary>
        /// Handles chat messages from clients
        /// </summary>
        /// <param name="chatString">Chat message received</param>
        public void HandleChatMessage(string chatString)
        {
            Console.WriteLine($"Client: {chatString}");
        }

        

    }
}
