using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OELib.LibraryBase;
using OELib.LibraryBase.Messages;
using ProtoBuf;

namespace OELibProtobufFormatter
{
    public class SerializationHelper
    {
        private readonly Dictionary<Guid, Tuple<Action<Stream, object>, Func<Stream, Guid, object>>> _manualSerDict =
            new Dictionary<Guid, Tuple<Action<Stream, object>, Func<Stream, Guid, object>>>();

        private readonly HashSet<Guid> _nonProtobufTypes = new HashSet<Guid>(); // known types that are not protobuf enabled

        private readonly HashSet<Guid> _protobufTypes = new HashSet<Guid>(); // known protobuf types


        public SerializationHelper()
        {
            SetupManualSerData();
        }


        public void WriteSerializationType(Stream stream, SerializationType type)
        {
            var t = new byte[1];
            t[0] = (byte) type;
            stream.Write(t, 0, 1);
        }

        public SerializationType ReadSerializationType(Stream stream)
        {
            var t = new byte[1];
            stream.Read(t, 0, 1);
            return (SerializationType) t[0];
        }

        public SerializationType DetermineApproprateSerialization(object obj)
        {
            if (obj is Bye || obj is Ping || obj is Pong)
                return SerializationType.Manual;
            var objType = obj.GetType();
            if (_protobufTypes.Contains(objType.GUID)) return SerializationType.Protobuf;
            if (_nonProtobufTypes.Contains(objType.GUID)) return SerializationType.Binary;
            var attrbs = objType.GetCustomAttributes(true).ToList();
            if (attrbs.Any(a => a is ProtoContractAttribute))
            {
                _protobufTypes.Add(objType.GUID);
                return SerializationType.Protobuf;
            }
            _nonProtobufTypes.Add(objType.GUID);
            return SerializationType.Binary;
        }

        public Guid ReadGuid(Stream stream)
        {
            var b = new byte[16];
            stream.Read(b, 0, 16);
            return new Guid(b);
        }

        public void WriteGuid(Stream stream, Guid guid)
        {
            var guidArr = guid.ToByteArray();
            stream.Write(guidArr, 0, 16);
        }

        public Priority ReadPrio(Stream stream)
        {
            var buffer = new byte[2];
            stream.Read(buffer, 0, 2);
            return (Priority) BitConverter.ToInt16(buffer, 0);
        }

        public void WritePrio(Stream stream, Priority priority)
        {
            stream.Write(BitConverter.GetBytes((short) priority), 0, 2);
        }


        public void SetupManualSerData()
        {
            void WriteGuidAndPrio(Stream stream, object obj)
            {
                var objType = obj.GetType();
                WriteGuid(stream, objType.GUID);
                WritePrio(stream, ((Message) obj).Priority);
            }

            _manualSerDict.Add(typeof(Bye).GUID, new Tuple<Action<Stream, object>, Func<Stream, Guid, object>>(
                WriteGuidAndPrio, (str, guid) => new Bye {Priority = ReadPrio(str)}));
            _manualSerDict.Add(typeof(Ping).GUID, new Tuple<Action<Stream, object>, Func<Stream, Guid, object>>(
                WriteGuidAndPrio, (str, guid) => new Ping {Priority = ReadPrio(str)}));
            _manualSerDict.Add(typeof(Pong).GUID, new Tuple<Action<Stream, object>, Func<Stream, Guid, object>>(
                WriteGuidAndPrio, (str, guid) => new Pong {Priority = ReadPrio(str)}));
            //todo: add manual serializations for poking collection messages
        }

        public void ManuallySerialize(Stream stream, object obj)
        {
            var objType = obj.GetType();
            if (!_manualSerDict.ContainsKey(objType.GUID))
                throw new InvalidOperationException("Cannot manually serialize this type");
            _manualSerDict[objType.GUID].Item1(stream, obj);
        }

        public object ManuallyDeserialize(Stream stream)
        {
            var guid = ReadGuid(stream);
            if (!_manualSerDict.ContainsKey(guid))
                throw new InvalidOperationException("Cannot manually serialize this type");
            return _manualSerDict[guid].Item2(stream, guid);
        }
    }
}