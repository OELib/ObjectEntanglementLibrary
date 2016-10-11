using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OELib.PokingConnection;
using SimpleExampleCommon;

namespace SimpleExampleServer
{
    /// <summary>
    /// A simple OELib server
    /// </summary>
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
        /// Prints the SomeData property of the receivedObject parameter. Used to test invokation from the client.
        /// </summary>
        /// <param name="receivedObject"></param>
        public void HandleTestObject(TestClass receivedObject)
        {
            Console.WriteLine("Received test object: {0}", receivedObject.SomeData);
        }

        /// <summary>
        /// Invokes the method "public void HandleAnotherTestClass(AnotherTestClass a)" on all connected clients.
        /// </summary>
        public void BroadCastToAllClients()
        {
            Console.WriteLine("Broadcasting a reply to all connected clients.");

            // Invoke the same method in all connected clients.
            // To achieve parallelism it use Parallel.ForEach from Task Parallel Library
            _server.Connections.ForEach((client) =>
            {
                client.Reactor.CallRemoteMethod("HandleAnotherTestClass", new AnotherTestClass() { SomeData = "Broadcast from server" });
            });
        }


        /// <summary>
        /// Handles a ping request and returns a Pong object to the caller.
        /// </summary>
        /// <returns>A Pong object</returns>
        public Pong HandlePing()
        {
            // A client invoked this method with a Ping
            Console.WriteLine("Received Ping. Replying with a Pong");

            // Return a Pong to the calling client
            return new Pong();
        }

        public LargeObject HandleLargeObject(LargeObject largeObject)
        {
            // A client invoked this method with a LargeObject
            Console.WriteLine("Received LargeObject. Returning it to the sender");

            // Return the LargeObject to the calling client
            return largeObject;
        }

        /// <summary>
        /// Example of void return method call
        /// </summary>
        /// <param name="message">Message to print</param>
        public void Echo(string message)
        {
            Console.WriteLine($"Message from client: {message}");
        }

        #region Tests generics
        public string TestGenerics()
        {
            return "Non generic method called";
        }

        public string TestGenerics<T>(T testObject)
        {
            return $"Generic type argument was: {testObject.GetType()}";
        }

        public string TestGenerics<T>(T testObject, T test2)
        {
            return $"Generic type argument was: {testObject.GetType()} and {test2.GetType()}";
        }

        public string TestGenerics<T>(T testObject, T test2, string testString)
        {
            return $"Generic type argument was: {testObject.GetType()} and {test2.GetType()}. Test string was: {testString}";
        }

        public string TestGenerics<T>()
        {
            return $"Generic type was: {typeof(T)}";
        }
        #endregion

    }
}
