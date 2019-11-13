using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OELib.UniversalConnection;

namespace OELibTests
{
    /// <summary>
    /// Universal connection tests
    /// </summary>
    [TestClass]
    public class UCTests
    {
        private const string _serverDir = @".\TestDirServer\";
        private const string _clientDir = @".\TestDirClient\";
        private const int _port = 12345;

        [TestInitialize]
        public void Init()
        {
            Directory.CreateDirectory(_serverDir);
            Directory.CreateDirectory(_clientDir);

            emptyDir();
        }

        private static void emptyDir()
        {
            var di = new DirectoryInfo(_serverDir);
            foreach (var file in di.GetFiles()) file.Delete();
            di = new DirectoryInfo(_clientDir);
            foreach (var file in di.GetFiles()) file.Delete();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Directory.Delete(_serverDir, true);
            Directory.Delete(_clientDir, true);
        }

        public class ServerReactor
        {
            public void TestMethod()
            {
            }
        }

        public class ClientReactor
        {
            public void TestMethod()
            {
            }
        }


        /// <summary>
        /// Tests that the Started event is called after the reactor has been started
        /// </summary>
        [TestMethod]
        public void ReactorConnectionTimingTest()
        {
            var sReactor = new ServerReactor();
            var cReactor = new ClientReactor();
            var server = new UcServer(_port, sReactor, _serverDir);
            server.Start();
            var client = new UCClientConnection(cReactor, _clientDir);
            var ok = new AutoResetEvent(false);
            client.Started += (_, __) => { client.Reactor.CallRemoteMethod("TestMethod");
                ok.Set();
            };
            client.Start("127.0.0.1", _port);
            Assert.IsTrue(ok.WaitOne(50));
            client.Stop();
            server.Stop();
        }


        /// <summary>
        /// This tests a that the client connected event does not produce a deadlock
        /// </summary>
        [TestMethod]
        public void ClientConnectedEventTest()
        {
            var sReactor = new ServerReactor();
            var cReactor = new ClientReactor();
            var server = new UcServer(_port, sReactor, _serverDir);
            server.Start();
            var serverConnectionEvent = new AutoResetEvent(false);
            server.ClientConnected += (_, __) =>
            {
                var numberOfClients = server.Connections.Count; // potential bug here
                Assert.AreEqual(1, numberOfClients);

                serverConnectionEvent.Set();
            };
            var client = new UCClientConnection(cReactor, _clientDir);
            var ok = new AutoResetEvent(false);
            client.Started += (_, __) => {
                client.Reactor.CallRemoteMethod("TestMethod");
                ok.Set();
            };
            client.Start("127.0.0.1", _port);
            Assert.IsTrue(ok.WaitOne(50));
            Assert.IsTrue(serverConnectionEvent.WaitOne(50));
            client.Stop();
            server.Stop();
        }




    }
}
