using System;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OELib.LibraryBase;
using OELib.PokingConnection.Messages;
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
            [ProtoMember(1)]
            public int I { get; set; }
        }




        [TestMethod]
        public void ManualSerialization()
        {
            var fmt1 = new OELibProtobufFormatter.OELibProtobufFormatter();
            var AssemblyQualifiedName = typeof(Echo).AssemblyQualifiedName;
            fmt1.SerializationHelper.ManualSerilaizationActions.Add(AssemblyQualifiedName,
                new Tuple<Action<System.IO.Stream, object>, Func<System.IO.Stream, string, object>>(
                    (s, o) => {  }, (s, g) => new Echo()));

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
            client.MessageReceived += (s, e) =>
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
            client.MessageReceived += (s, e) =>
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
            client.MessageReceived += (s, e) =>
            {
                if (e is Echo3) go2.Set();
            };

            server.Connections.First().SendMessage(new Echo3());
            var ok2 = go2.WaitOne(500);
            Assert.IsTrue(ok2);

        }



        [TestMethod]
        public void MixedSerialization()
        {
            var fmt1 = new OELibProtobufFormatter.OELibProtobufFormatter();
            var AssemblyQualifiedName = typeof(Echo).AssemblyQualifiedName;
            fmt1.SerializationHelper.ManualSerilaizationActions.Add(AssemblyQualifiedName,
                new Tuple<Action<System.IO.Stream, object>, Func<System.IO.Stream, string, object>>(
                    (s, o) => {  }, (s, g) => new Echo()));
            var go = new AutoResetEvent(false);
            var server =
                new CommunicationServer<ServerSideConnection>(new IPEndPoint(IPAddress.Any, 1028))
                {
                    Formatter = fmt1
                };
            server.Start();
            server.ClientConnected += (s, e) => go.Set();
            var client = new ClientSideConnection(fmt1);
            client.Start("127.0.0.1", 1028);
            var ok = go.WaitOne(500);
            Assert.IsTrue(ok);
            var go2 = new AutoResetEvent(false);
            var go3 = new AutoResetEvent(false);
            var go4 = new AutoResetEvent(false);
            client.MessageReceived += (s, e) =>
            {
                if (e is Echo) go2.Set();
                if (e is Echo2) go3.Set();
                if (e is Echo3) go4.Set();
            };
            server.Connections.First().SendMessage(new Echo());
            server.Connections.First().SendMessage(new Echo2());
            server.Connections.First().SendMessage(new Echo3());
            var ok2 = go2.WaitOne(500);
            Assert.IsTrue(ok2);
            var ok3 = go3.WaitOne(500);
            Assert.IsTrue(ok3);
            var ok4 = go4.WaitOne(500);
            Assert.IsTrue(ok4);
            server.Connections.First().SendMessage(new Echo3());
            server.Connections.First().SendMessage(new Echo2());
            server.Connections.First().SendMessage(new Echo());
            ok2 = go2.WaitOne(500);
            Assert.IsTrue(ok2);
            ok3 = go3.WaitOne(500);
            Assert.IsTrue(ok3);
            ok4 = go4.WaitOne(500);
            Assert.IsTrue(ok4);
        }
        
        [TestMethod]
        public void CallMethodSerialization()
        {
            var fmt1 = new OELibProtobufFormatter.OELibProtobufFormatter();
            var assemblyQualifiedName = typeof(Echo).AssemblyQualifiedName; 
            // ReSharper disable once AssignNullToNotNullAttribute
            fmt1.SerializationHelper.ManualSerilaizationActions.Add(assemblyQualifiedName,
                new Tuple<Action<System.IO.Stream, object>, Func<System.IO.Stream, string, object>>(
                    (s, o) => {  }, (s, g) => new Echo()));
            var go = new AutoResetEvent(false);
            var server =
                new CommunicationServer<ServerSideConnection>(new IPEndPoint(IPAddress.Any, 1029))
                {
                    Formatter = fmt1
                };
            server.Start();
            server.ClientConnected += (s, e) => go.Set();
            var client = new ClientSideConnection(fmt1);
            client.Start("127.0.0.1", 1029);
            var ok = go.WaitOne(500);
            Assert.IsTrue(ok);
            var go2 = new AutoResetEvent(false);
            CallMethod ee = null;
            client.MessageReceived += (s, e) =>
            {
                ee = e as CallMethod;
                if (ee != null) go2.Set();
                
            };
            server.Connections.First().SendMessage(new CallMethod("SomeMethodName", new object[]{1, 2.2, "hello call method", new Echo(), new Echo2(), new Echo3()}, null));
            var ok2 = go2.WaitOne(500);
            Assert.IsTrue(ok2);
            Assert.AreEqual("SomeMethodName", ee.MethodName);
            Assert.AreEqual("hello call method", (string) ee.Arguments[2]);
        }

        [TestMethod]
        public void CallMethodResponseSerialization()
        {
            var fmt1 = new OELibProtobufFormatter.OELibProtobufFormatter();
            var assemblyQualifiedName = typeof(Echo).AssemblyQualifiedName;
            // ReSharper disable once AssignNullToNotNullAttribute
            fmt1.SerializationHelper.ManualSerilaizationActions.Add(assemblyQualifiedName,
                new Tuple<Action<System.IO.Stream, object>, Func<System.IO.Stream, string, object>>(
                    (s, o) => {  }, (s, g) => new Echo()));
            var go = new AutoResetEvent(false);
            var server =
                new CommunicationServer<ServerSideConnection>(new IPEndPoint(IPAddress.Any, 1030))
                {
                    Formatter = fmt1
                };
            server.Start();
            server.ClientConnected += (s, e) => go.Set();
            var client = new ClientSideConnection(fmt1);
            client.Start("127.0.0.1", 1030);
            var ok = go.WaitOne(500);
            Assert.IsTrue(ok);
            var go2 = new AutoResetEvent(false);
            CallMethodResponse ee = null;
            client.MessageReceived += (s, e) =>
            {
                ee = e as CallMethodResponse;
                if (ee != null) go2.Set();

            };
            server.Connections.First().SendMessage(
                new CallMethodResponse(new CallMethod("MethodName", new object[] {}, null), new Echo3(), new NullReferenceException())
            );
            var ok2 = go2.WaitOne(500);
            Assert.IsTrue(ok2);
            Assert.IsTrue(ee.Response is Echo3);
            Assert.IsTrue(ee.Exception is NullReferenceException);
        }



    }
}
