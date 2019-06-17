using System;
using System.Threading;
using OELib.UniversalConnection;

namespace UCExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var serverReactingObject = new ExampleReactingObject() { Name = "Server reacting object" };
            var clientReactingObject = new ExampleReactingObject() { Name = "Client reacting object" };
            var serverExampleObject = new ExampleObject() { Name = "Server example object" };
            var clientExampleObject = new ExampleObject() {Name = "Client example object"};
            var server = new UcServer(1024, serverReactingObject);
            server.ClientConnected += (_, __) => Console.WriteLine("A client connected to the server");
            server.Start();
            var client = new UCClientConnection(clientReactingObject);
            client.ObjectReceived += (sender, obj) => Console.WriteLine($"Client got an object: {obj}");
            server.ObjectReceived += (sender, obj) => Console.WriteLine($"Server got an object: {obj}");

            AutoResetEvent go = new AutoResetEvent(false);
            client.Started += (_, __) => go.Set();
            client.Start("127.0.0.1", 1024);
            go.WaitOne(10000);
            client.Reactor.CallRemoteMethod(OELib.LibraryBase.Priority.Normal, 100, "ExampleMethod",
                "Method executed on server, called on client");
            server.Connections.ForEach(c => c.Reactor.CallRemoteMethod(OELib.LibraryBase.Priority.Normal, 100,
                "ExampleMethod", "Method executed on client, called on server"));
            server.SendObject(serverExampleObject);
            client.SendObject(clientExampleObject);
            Thread.Sleep(100);
            client.Stop();
            server.Stop();
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
    }
}
