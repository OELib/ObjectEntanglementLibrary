using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OELib.LibraryBase;
using OELib.LibraryBase.Messages;
using OELibProtobufFormatter;
using ProtoBuf;

namespace OELibProtobufFormatterTests
{
    [TestClass]
    public class SerializationTest
    {
        private SerializationHelper _sh;

        [TestInitialize]
        public void InitTest()
        {
            _sh = new SerializationHelper();
        }

        [TestMethod]
        public void ReadWriteSerializationType()
        {
            var ms = new MemoryStream();
            _sh.WriteSerializationType(ms, SerializationType.Binary);
            _sh.WriteSerializationType(ms, SerializationType.Manual);
            _sh.WriteSerializationType(ms, SerializationType.Protobuf);

            ms.Seek(0, SeekOrigin.Begin);
            var st = _sh.ReadSerializationType(ms);
            Assert.AreEqual(SerializationType.Binary, st);
            st = _sh.ReadSerializationType(ms);
            Assert.AreEqual(SerializationType.Manual, st);
            st = _sh.ReadSerializationType(ms);
            Assert.AreEqual(SerializationType.Protobuf, st);
        }


        [TestMethod]
        public void DetermineApproprateSerialization()
        {
            var a = new Bye();
            var b = new Ping();
            var c = new Pong();
            var d = new DummyMessage();
            var e = new DummyProtoMessage();

            var at = _sh.DetermineApproprateSerialization(a);
            var bt = _sh.DetermineApproprateSerialization(b);
            var ct = _sh.DetermineApproprateSerialization(c);
            var dt = _sh.DetermineApproprateSerialization(d);
            var et = _sh.DetermineApproprateSerialization(e);

            Assert.AreEqual(SerializationType.Manual, at);
            Assert.AreEqual(SerializationType.Manual, bt);
            Assert.AreEqual(SerializationType.Manual, ct);
            Assert.AreEqual(SerializationType.Binary, dt);
            Assert.AreEqual(SerializationType.Protobuf, et);
        }

        [TestMethod]
        public void ReadWriteGuid()
        {
            var g = new Guid(new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 7, 6, 5, 4, 3, 2, 1, 0});
            var ms = new MemoryStream();
            _sh.WriteGuid(ms, g);
            ms.Seek(0, SeekOrigin.Begin);
            var gds = _sh.ReadGuid(ms);
            Assert.AreEqual(g, gds);
        }

        [TestMethod]
        public void ReadWritePrio()
        {
            var p = Priority.High;
            var ms = new MemoryStream();
            _sh.WritePrio(ms, p);
            ms.Seek(0, SeekOrigin.Begin);
            var pds = _sh.ReadPrio(ms);
            Assert.AreEqual(p, pds);
            ms.Seek(0, SeekOrigin.Begin);
            p = Priority.Normal;
            _sh.WritePrio(ms, p);
            ms.Seek(0, SeekOrigin.Begin);
            pds = _sh.ReadPrio(ms);
            Assert.AreEqual(p, pds);
        }

        [TestMethod]
        public void ReadWriteManuallySerMsgs()
        {
            var a = new Bye {Priority = Priority.High};
            var b = new Ping {Priority = Priority.High};
            var c = new Pong {Priority = Priority.High};
            var d = new Bye {Priority = Priority.Normal};
            var e = new Ping {Priority = Priority.Normal};
            var f = new Pong {Priority = Priority.Normal};
            var ms = new MemoryStream();
            _sh.ManuallySerialize(ms, a);
            _sh.ManuallySerialize(ms, b);
            _sh.ManuallySerialize(ms, c);
            _sh.ManuallySerialize(ms, d);
            _sh.ManuallySerialize(ms, e);
            _sh.ManuallySerialize(ms, f);
            ms.Seek(0, SeekOrigin.Begin);
            var ads = _sh.ManuallyDeserialize(ms);
            var bds = _sh.ManuallyDeserialize(ms);
            var cds = _sh.ManuallyDeserialize(ms);
            var dds = _sh.ManuallyDeserialize(ms);
            var eds = _sh.ManuallyDeserialize(ms);
            var fds = _sh.ManuallyDeserialize(ms);
            Assert.IsInstanceOfType(ads, typeof(Bye));
            Assert.IsInstanceOfType(bds, typeof(Ping));
            Assert.IsInstanceOfType(cds, typeof(Pong));
            Assert.IsInstanceOfType(dds, typeof(Bye));
            Assert.IsInstanceOfType(eds, typeof(Ping));
            Assert.IsInstanceOfType(fds, typeof(Pong));
            Assert.AreEqual(Priority.High, ((Message) ads).Priority);
            Assert.AreEqual(Priority.High, ((Message) bds).Priority);
            Assert.AreEqual(Priority.High, ((Message) cds).Priority);
            Assert.AreEqual(Priority.Normal, ((Message) dds).Priority);
            Assert.AreEqual(Priority.Normal, ((Message) eds).Priority);
            Assert.AreEqual(Priority.Normal, ((Message) fds).Priority);
        }

        public class DummyMessage : Message
        {
        }

        [ProtoContract]
        public class DummyProtoMessage : Message
        {
        }
    }
}