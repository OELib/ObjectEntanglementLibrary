using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using OELib.LibraryBase;
using OELib.LibraryBase.Messages;
using ProtoBuf;

namespace OELibProtobufFormatter
{
    public class SerializationHelper
    {
        public Dictionary<Guid, Tuple<Action<Stream, object>, Func<Stream, Guid, object>>> ManualSerilaizationActions { get; } =
            new Dictionary<Guid, Tuple<Action<Stream, object>, Func<Stream, Guid, object>>>();

        private readonly HashSet<Guid> _nonProtobufTypes = new HashSet<Guid>(); // known types that are not protobuf enabled

        private readonly HashSet<Guid> _protobufTypes = new HashSet<Guid>(); // known protobuf types
        private BinaryFormatter _bf;
        


        public SerializationHelper()
        {
            SetupManualSerData();
            _bf = new BinaryFormatter();
            
        }


        public static void WriteSerializationType(Stream stream, SerializationType type)
        {
            var t = new byte[1];
            t[0] = (byte) type;
            stream.Write(t, 0, 1);
        }

        public static SerializationType ReadSerializationType(Stream stream)
        {
            var t = new byte[1];
            stream.Read(t, 0, 1);
            return (SerializationType) t[0];
        }

        public SerializationType DetermineApproprateSerialization(object obj)
        {
            if (ManualSerilaizationActions.ContainsKey(obj.GetType().GUID))
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

        public static Guid ReadGuid(Stream stream)
        {
            var b = new byte[16];
            stream.Read(b, 0, 16);
            return new Guid(b);
        }

        public static void WriteGuid(Stream stream, Guid guid)
        {
            var guidArr = guid.ToByteArray();
            stream.Write(guidArr, 0, 16);
        }

        public static short ReadShort(Stream stream)
        {
            var buffer = new byte[2];
            stream.Read(buffer, 0, 2);
            return BitConverter.ToInt16(buffer, 0);
        }

        public static void WriteShort(Stream stream, short data)
        {
            stream.Write(BitConverter.GetBytes(data), 0, 2);
        }

        /// <summary>
        /// Writes a null-terminated ASCII string to a stream
        /// </summary>
        /// <param name="stream">stream to write to</param>
        /// <param name="str">string to encode</param>
        public static void WriteString(Stream stream, string str)
        {
            var b = Encoding.ASCII.GetBytes(str).ToList();
            b.Add(0);
            stream.Write(b.ToArray(), 0, b.Count);
        }


        public static string ReadString(Stream stream)
        {
            var r = new StringBuilder();
            var c = new byte[1]; //TODO: fix this (it's inefficient)
            do
            {
                stream.Read(c, 0, 1);
                if (c[0] != 0)
                    r.Append((char)c[0]);
            } while (c[0] != 0);
            return r.ToString();
        }
        

        public void SetupManualSerData()
        {
            void WriteGuidAndPrio(Stream stream, object obj)
            {
                var objType = obj.GetType();
                WriteGuid(stream, objType.GUID);
                WriteShort(stream, (short)((Message) obj).Priority);
            }

            ManualSerilaizationActions.Add(typeof(Bye).GUID, new Tuple<Action<Stream, object>, Func<Stream, Guid, object>>(
                WriteGuidAndPrio, (str, guid) => new Bye {Priority = (Priority)ReadShort(str)}));
            ManualSerilaizationActions.Add(typeof(Ping).GUID, new Tuple<Action<Stream, object>, Func<Stream, Guid, object>>(
                WriteGuidAndPrio, (str, guid) => new Ping {Priority = (Priority)ReadShort(str)}));
            ManualSerilaizationActions.Add(typeof(Pong).GUID, new Tuple<Action<Stream, object>, Func<Stream, Guid, object>>(
                WriteGuidAndPrio, (str, guid) => new Pong {Priority = (Priority)ReadShort(str)}));
            //todo: add manual serializations for poking collection messages
        }

        public void ManuallySerialize(Stream stream, object obj)
        {
            var objType = obj.GetType();
            if (!ManualSerilaizationActions.ContainsKey(objType.GUID))
                throw new InvalidOperationException("Cannot manually serialize this type");
            ManualSerilaizationActions[objType.GUID].Item1(stream, obj);
        }

        public object ManuallyDeserialize(Stream stream)
        {
            var guid = ReadGuid(stream);
            if (!ManualSerilaizationActions.ContainsKey(guid))
                throw new InvalidOperationException("Cannot manually serialize this type");
            return ManualSerilaizationActions[guid].Item2(stream, guid);
        }


        public void BinarySerialize(Stream stream, object obj)
        {
            _bf.Serialize(stream, obj);
        }

        public object BinaryDeserialize(Stream stream)
        {
            return _bf.Deserialize(stream);
        }

        public void ProtobufSerialize(Stream stream, object obj)
        {
            //TODO: this is very slow. calls should be cashed
            var objT = obj.GetType();
            WriteString(stream, objT.AssemblyQualifiedName); // protobuf deserialize needs to know the type
            var methodInfo = typeof(Serializer).GetMethods(System.Reflection.BindingFlags.Public |
                                                           System.Reflection.BindingFlags
                                                               .Static).Where(mi => mi.Name == "Serialize").FirstOrDefault(mi => mi.GetParameters().Count() == 2 && mi.GetParameters()[0].ParameterType.Name == "Stream" &&
                             mi.GetParameters()[1].ParameterType.Name == "T"); // todo: how can this be done better
            var methGenericInfo = methodInfo?.MakeGenericMethod(objT);
            methGenericInfo?.Invoke(null, new[] {stream, obj});
        }

        public object ProtobufDeserialize(Stream stream)
        {
            //TODO: this is very slow. calls should be cashed
            var typeName = ReadString(stream);
            var type = Type.GetType(typeName);
            var methodInfo = typeof(Serializer).GetMethods(System.Reflection.BindingFlags.Public |
                                                           System.Reflection.BindingFlags
                                                               .Static).Where(mi => mi.Name == "Deserialize").FirstOrDefault(mi=>mi.GetParameters().Count() == 2
                                                               && mi.GetParameters()[0].ParameterType.Name == "Type" && mi.GetParameters()[1].ParameterType.Name == "Stream" );
            return methodInfo?.Invoke(null, new object[] {type, stream});
        }

    }
}