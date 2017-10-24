using System;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OELib.PokingConnection.ObjectTunnel;


namespace OELibTests
{
    [TestClass]
    public class ObjectTunnelTest
    {
        [Serializable]
        public class TestPayload
        {
            public double D { get; set; }
            public int I { get; set; }
            public string S { get; set; }
        }


        [TestMethod]
        public void SendRecieveThroughTunnel()
        {
            var tp = new TestPayload() {D = 2.2, I = 1, S = "Wave"};
            var server = new ObjectTunnelServer(1044);
            var client = new ObjectTunnelClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1044));
            var go1 = new AutoResetEvent(false);
            var go2 = new AutoResetEvent(false);
            var go3 = new AutoResetEvent(false);
            client.Connected += (s, e) =>
            {
                go1.Set();
            };
            client.StartConnectionAttempts();
            Assert.IsTrue(go1.WaitOne(100));
            server.ObjectReceived += (s, o) =>
            {
                go2.Set();
            };
            client.ObjectReceived += (s, o) =>
            {
                go3.Set();
            };

            client.SendObject(tp);
            Assert.IsTrue(go2.WaitOne(100));
            server.SendObject(tp);
            Assert.IsTrue(go3.WaitOne(100));
        }
    }
}
