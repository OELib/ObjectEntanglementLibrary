﻿using OELib.PokingConnection;
using SimpleExampleCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleExampleClient
{
    /// <summary>
    /// A simple OELib client
    /// </summary>
    public class SimpleClient
    {
        private PokingClientConnection _client;

        /// <summary>
        /// Constructor for the SimpleClient. Creates a client and initiates the connection procedure.
        /// </summary>
        /// <param name="serverAddress">Ip address of the OELib server to connect to</param>
        /// <param name="serverPort">Port of the OELib server to connect to</param>
        public SimpleClient(string serverAddress, int serverPort)
        {
            // Create a new PokingClientConnection
            _client = new PokingClientConnection(this);

            // Connect the client to the server
            _client.Start(serverAddress, serverPort);

            // When the client and server have agreed on which methods are supported it is possible to send objects to the other part.
            _client.Reactor.RemoteReactingInspectionComplete += (s, e) =>
            {
                Console.WriteLine("Connected to server.");
                //TestGenerics();
                //SendTestObjectToServer();
            };
        }

        private void TestGenerics()
        {
            var nonGeneric = _client.Reactor.IsMethodSupported("TestGenerics");
            var generic = _client.Reactor.IsMethodSupported("TestGenerics<T>");

            var testNonGeneric = _client.Reactor.CallRemoteMethod(_client.Reactor.DefaultCallPriority, _client.Reactor.DefaultCallTimeout, "TestGenerics");
            Console.WriteLine(testNonGeneric as string);

            var testGenerics = _client.Reactor.CallRemoteMethod<TestClass>(_client.Reactor.DefaultCallPriority, _client.Reactor.DefaultCallTimeout, "TestGenerics");
            Console.WriteLine(testGenerics as string);

            var testGenerics2 = _client.Reactor.CallRemoteMethod<TestClass>(_client.Reactor.DefaultCallPriority, _client.Reactor.DefaultCallTimeout, "TestGenerics", new TestClass());
            Console.WriteLine(testGenerics2 as string);

            var testGenerics3 = _client.Reactor.CallRemoteMethod<TestClass>(_client.Reactor.DefaultCallPriority, _client.Reactor.DefaultCallTimeout, "TestGenerics", new TestClass(), new TestClass());
            Console.WriteLine(testGenerics3 as string);

            var testGenerics4 = _client.Reactor.CallRemoteMethod<TestClass>(_client.Reactor.DefaultCallPriority, _client.Reactor.DefaultCallTimeout, "TestGenerics", new TestClass(), new TestClass(), "Hello generics");
            Console.WriteLine(testGenerics4 as string);
        }


        /// <summary>
        /// Invokes the method "public void HandleTestObject(TestClass t)" on the server's reacting object
        /// </summary>
        public void SendTestObjectToServer()
        {
            // Invoke the method HandleTestObject method on the server's reacting object
            _client.Reactor.CallRemoteMethod("HandleTestObject", new TestClass() { SomeData = "Test object" });
        }

        /// <summary>
        /// Invokes the method "public Pong HandlePing()" on the server and waits for the Pong reply. 
        /// If a reply is received before timing out the time for the entire Ping-Pong is printed to the console.
        /// </summary>
        public void SendPing()
        {
            Console.WriteLine("Sending a ping");
            Stopwatch sw = Stopwatch.StartNew();
            var reply = _client.Reactor.CallRemoteMethod("HandlePing") as Pong;
            if(reply != null)
                Console.WriteLine("Received Pong after {0} ms", sw.ElapsedMilliseconds);
            else
                Console.WriteLine("Did not receive Pong within timeout");
        }


        public void SendLargeObject()
        {
            Console.WriteLine("Sending a large object");
            Stopwatch sw = Stopwatch.StartNew();
            var reply = _client.Reactor.CallRemoteMethod("HandleLargeObject", new LargeObject()) as LargeObject;
            if (reply != null)
                Console.WriteLine("Received large object reply after {0} ms", sw.ElapsedMilliseconds);
            else
                Console.WriteLine("Did not receive large object within timeout");
        }

        /// <summary>
        /// Prints the SomeData property of the testObject parameter. Used to test invokation from the server.
        /// </summary>
        /// <param name="testObject">Test object to handle</param>
        public void HandleAnotherTestClass(AnotherTestClass testObject)
        {
            Console.WriteLine("Received message: {0}", testObject.SomeData);
        }

        /// <summary>
        /// Calls a void method on server but does not wait for the replay
        /// </summary>
        public void CallVoidMethodFireAndForget()
        {
            _client.Reactor.CallRemoteVoidMethod(OELib.LibraryBase.Priority.Normal, "Echo", $"Local time is {DateTime.Now.ToShortTimeString()}.");
        }

    }
}
