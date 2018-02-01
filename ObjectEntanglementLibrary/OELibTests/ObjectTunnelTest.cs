using System;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OELib.ObjectTunnel;


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
            var server = new ObjectTunnelServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1044));
            server.Start();
            var client = new ObjectTunnelClientConnection();
            var go1 = new AutoResetEvent(false);
            var go2 = new AutoResetEvent(false);
            var go3 = new AutoResetEvent(false);
            client.Started += (s, e) =>
            {
                go1.Set();
            };
            Assert.IsTrue(client.Start("127.0.0.1", 1044));
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

            Assert.IsTrue(client.SendObject(tp1));
            Assert.IsTrue(go2.WaitOne(500));
            Assert.AreEqual(tp1.D, tp2.D, 1e-15);
            Assert.AreEqual(tp1.I, tp2.I);
            Assert.AreEqual(tp1.S, tp2.S);

            server.SendObject(tp1);
            Assert.IsTrue(go3.WaitOne(500));
            Assert.AreEqual(tp1.D, tp3.D, 1e-15);
            Assert.AreEqual(tp1.I, tp3.I);
            Assert.AreEqual(tp1.S, tp3.S);
        }
    }
}
