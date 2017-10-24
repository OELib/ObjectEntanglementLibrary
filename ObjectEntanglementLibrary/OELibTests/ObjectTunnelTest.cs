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
            var tp1 = new TestPayload() {D = 2.2, I = 1, S = "Wave"};
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
            TestPayload tp2 = null, tp3 = null;
            server.ObjectReceived += (s, o) =>
            {
                tp2 = o as TestPayload;
                go2.Set();
            };
            client.ObjectReceived += (s, o) =>
            {
                tp3 = o as TestPayload;
                go3.Set();
            };

            client.SendObject(tp1);
            Assert.IsTrue(go2.WaitOne(100));
            Assert.AreEqual(tp1.D, tp2.D, 1e-15);
            Assert.AreEqual(tp1.I, tp2.I);
            Assert.AreEqual(tp1.S, tp2.S);

            server.SendObject(tp1);
            Assert.IsTrue(go3.WaitOne(100));
            Assert.AreEqual(tp1.D, tp3.D, 1e-15);
            Assert.AreEqual(tp1.I, tp3.I);
            Assert.AreEqual(tp1.S, tp3.S);
        }
    }
}
