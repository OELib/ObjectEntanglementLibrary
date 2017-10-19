using System;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OELib.LibraryBase;
using OELibProtobufFormatter;
using ProtoBuf;


namespace OELibProtobufFormatterTests
{
    [TestClass]
    public class OELibWithProtobufTest
    {
        [TestMethod]
        public void Connection()
        {
            var go = new AutoResetEvent(false);
            var server =
                new CommunicationServer<ServerSideConnection>(new IPEndPoint(IPAddress.Any, 1024))
                {
                    Formatter = new OELibProtobufFormatter.OELibProtobufFormatter()
                };
            server.Start();
            server.ClientConnected += (s, e) =>
            {
                go.Set();
            };


            var client = new ClientSideConnection(new OELibProtobufFormatter.OELibProtobufFormatter());
            client.Start("127.0.0.1", 1024);
            var ok = go.WaitOne(500);
            Assert.IsTrue(ok);
            server.Stop();
        }
        
        public class Echo : OELib.LibraryBase.Messages.Message
        {
            
        }

        [Serializable]
        public class Echo2 : OELib.LibraryBase.Messages.Message
        {

        }

        [ProtoContract]
        public class Echo3 : OELib.LibraryBase.Messages.Message
        {

        }




        [TestMethod]
        public void ManualSerialization()
        {
            var fmt1 = new OELibProtobufFormatter.OELibProtobufFormatter();
            var echoGuid = typeof(Echo).GUID;
            fmt1.SerializationHelper.ManualSerilaizationActions.Add(echoGuid,
                new Tuple<Action<System.IO.Stream, object>, Func<System.IO.Stream, Guid, object>>(
                    (s, o) => { SerializationHelper.WriteGuid(s, echoGuid); }, (s, g) =>
                    {
                        SerializationHelper.ReadGuid(s); return new Echo(); }));

            var go = new AutoResetEvent(false);
            var server =
                new CommunicationServer<ServerSideConnection>(new IPEndPoint(IPAddress.Any, 1025))
                {
                    Formatter = fmt1
                };
            server.Start();
            server.ClientConnected += (s, e) => go.Set();
            var client = new ClientSideConnection(fmt1);
            client.Start("127.0.0.1", 1025);
            var ok = go.WaitOne(500);
            Assert.IsTrue(ok);
            var go2 = new AutoResetEvent(false);
            client.MessageRecieved += (s, e) =>
            {
                if (e is Echo) go2.Set();
            };

            server.Connections.First().SendMessage(new Echo());
            var ok2 = go2.WaitOne(500);
            Assert.IsTrue(ok2);

        }

        [TestMethod]
        public void BinarySerialization()
        {
            var fmt1 = new OELibProtobufFormatter.OELibProtobufFormatter();
            
            var go = new AutoResetEvent(false);
            var server =
                new CommunicationServer<ServerSideConnection>(new IPEndPoint(IPAddress.Any, 1026))
                {
                    Formatter = fmt1
                };
            server.Start();
            server.ClientConnected += (s, e) => go.Set();
            var client = new ClientSideConnection(fmt1);
            client.Start("127.0.0.1", 1026);
            var ok = go.WaitOne(500);
            Assert.IsTrue(ok);
            var go2 = new AutoResetEvent(false);
            client.MessageRecieved += (s, e) =>
            {
                if (e is Echo2) go2.Set();
            };

            server.Connections.First().SendMessage(new Echo2());
            var ok2 = go2.WaitOne(500);
            Assert.IsTrue(ok2);

        }

        [TestMethod]
        public void ProtobufSerialization()
        {
            var fmt1 = new OELibProtobufFormatter.OELibProtobufFormatter();

            var go = new AutoResetEvent(false);
            var server =
                new CommunicationServer<ServerSideConnection>(new IPEndPoint(IPAddress.Any, 1027))
                {
                    Formatter = fmt1
                };
            server.Start();
            server.ClientConnected += (s, e) => go.Set();
            var client = new ClientSideConnection(fmt1);
            client.Start("127.0.0.1", 1027);
            var ok = go.WaitOne(500);
            Assert.IsTrue(ok);
            var go2 = new AutoResetEvent(false);
            client.MessageRecieved += (s, e) =>
            {
                if (e is Echo3) go2.Set();
            };

            server.Connections.First().SendMessage(new Echo3());
            var ok2 = go2.WaitOne(500);
            Assert.IsTrue(ok2);

        }

    }
}
