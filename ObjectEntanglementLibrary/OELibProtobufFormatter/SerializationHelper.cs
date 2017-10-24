using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using OELib.LibraryBase;
using OELib.LibraryBase.Messages;
using OELib.PokingConnection.Messages;
using ProtoBuf;

namespace OELibProtobufFormatter
{

    public static class StreamUtils
    {
        public static byte[] ReadExactly(this Stream stream, int count)
        {
            var buffer = new byte[count];
            var offset = 0;
            while (offset < count)
            {
                var read = stream.Read(buffer, offset, count - offset);
                if (read == 0)
                    throw new EndOfStreamException();
                offset += read;
            }
            System.Diagnostics.Debug.Assert(offset == count);
            return buffer;
        }


        public static void CopyBytesTo(this Stream stream, Stream destination, int count)
        {
            var b = stream.ReadExactly(count);
            destination.Write(b, 0, count);
        }


    }


    public class SerializationHelper
    {
        public Dictionary<string, Tuple<Action<Stream, object>, Func<Stream, string, object>>> ManualSerilaizationActions { get; } =
            new Dictionary<string, Tuple<Action<Stream, object>, Func<Stream, string, object>>>();

        private readonly HashSet<Guid> _nonProtobufTypes = new HashSet<Guid>(); // known types that are not protobuf enabled

        private readonly HashSet<Guid> _protobufTypes = new HashSet<Guid>(); // known protobuf types
        private readonly BinaryFormatter _bf;



        public SerializationHelper()
        {
            SetupManualSerData();
            _bf = new BinaryFormatter();

        }


        public static void WriteSerializationType(Stream stream, SerializationType type)
        {
            var t = new byte[2];
            t[0] = (byte)type;
            t[1] = (byte)type;
            stream.Write(t, 0, 2);
        }

        public static SerializationType ReadSerializationType(Stream stream)
        {
            var t = stream.ReadExactly(2);
            if (t[0] != t[1])
                throw new DataMisalignedException();
            return (SerializationType)t[0];
        }
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public SerializationType DetermineApproprateSerialization(object obj)
        {
            if (obj == null) return SerializationType.Manual;
            if (ManualSerilaizationActions.ContainsKey(obj.GetType().AssemblyQualifiedName))
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
            var b = stream.ReadExactly(16);
            return new Guid(b);
        }

        public static void WriteGuid(Stream stream, Guid guid)
        {
            var guidArr = guid.ToByteArray();
            stream.Write(guidArr, 0, 16);
        }

        public static short ReadShort(Stream stream)
        {
            var buffer = stream.ReadExactly(2);
            return BitConverter.ToInt16(buffer, 0);
        }

        public static void WriteShort(Stream stream, short data)
        {
            stream.Write(BitConverter.GetBytes(data), 0, 2);
        }

        /// <summary>
        /// Writes a unicode string to a stream (first two bytes = length)
        /// </summary>
        /// <param name="stream">stream to write to</param>
        /// <param name="str">string to write</param>
        public static void WriteString(Stream stream, string str)
        {
            var bb = Encoding.Unicode.GetBytes(str).ToArray();
            WriteShort(stream, (short)bb.Length);
            stream.Write(bb, 0, bb.Length);
        }

        /// <summary>
        /// Reads an unicode string from the stream (first two bytese = length)
        /// </summary>
        /// <param name="stream">stream to read from</param>
        /// <returns>the string</returns>
        public static string ReadString(Stream stream)
        {
            var len = ReadShort(stream);
            var b = stream.ReadExactly(len);
            return Encoding.Unicode.GetString(b);
        }


        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void SetupManualSerData()
        {
            void WritePrio(Stream stream, object obj)
            {
                WriteShort(stream, (short)((Message)obj).Priority);
            }

            //system messages
            ManualSerilaizationActions.Add(typeof(Bye).AssemblyQualifiedName, new Tuple<Action<Stream, object>, Func<Stream, string, object>>(
                WritePrio, (str, guid) => new Bye { Priority = (Priority)ReadShort(str) }));
            ManualSerilaizationActions.Add(typeof(Ping).AssemblyQualifiedName, new Tuple<Action<Stream, object>, Func<Stream, string, object>>(
                WritePrio, (str, guid) => new Ping { Priority = (Priority)ReadShort(str) }));
            ManualSerilaizationActions.Add(typeof(Pong).AssemblyQualifiedName, new Tuple<Action<Stream, object>, Func<Stream, string, object>>(
                WritePrio, (str, guid) => new Pong { Priority = (Priority)ReadShort(str) }));
            ManualSerilaizationActions.Add("null", new Tuple<Action<Stream, object>, Func<Stream, string, object>>(
                (s, e) => { }, (str, guid) => null));


            //poking connection messages
            ManualSerilaizationActions.Add(typeof(CallMethod).AssemblyQualifiedName,
                new Tuple<Action<Stream, object>, Func<Stream, string, object>>(
                    (s, o) => // serialization
                    {
                        var oo = (CallMethod)o;
                        WriteGuid(s, oo.MessageID);
                        WriteGuid(s, oo.CallingMessageID);
                        WriteString(s, oo.MethodName);
                        WriteShort(s, (short)oo.Priority);
                        WriteShort(s, (short)oo.Arguments.Count());
                        oo.Arguments.ToList().ForEach(arg =>
                        {
                            Serialize(s, arg);
                        });
                    },
                    (s, g) => //deserialization
                    {
                        var messageID = ReadGuid(s);
                        var callingMessageID = ReadGuid(s);
                        var methodName = ReadString(s);
                        var priority = (Priority)ReadShort(s);
                        var argsCount = ReadShort(s);
                        var args = Enumerable.Range(0, argsCount).Select(i =>
                        {
                            var o = Deserialize(s);
                            return o;

                        }).ToArray();
                        var r = new CallMethod(methodName, args, null)
                        {
                            MessageID = messageID,
                            CallingMessageID = callingMessageID,
                            Priority = priority
                        }; //TODO: Generic types not supported, do it
                        return r;
                    }

            ));

            ManualSerilaizationActions.Add(typeof(CallMethodResponse).AssemblyQualifiedName,
                new Tuple<Action<Stream, object>, Func<Stream, string, object>>(
                    (s, o) => // serialization
                    {
                        var oo = (CallMethodResponse)o;
                        WriteGuid(s, oo.MessageID);
                        WriteGuid(s, oo.CallingMessageID);
                        WriteShort(s, (short)oo.Priority);
                        Serialize(s, oo.Response);
                        Serialize(s, oo.Exception);
                    },
                    (s, g) => //deserialization
                    {
                        var messageID = ReadGuid(s);
                        var callingMessageID = ReadGuid(s);
                        var priority = (Priority)ReadShort(s);
                        var response = Deserialize(s);
                        var exception = Deserialize(s);
                        var r = new CallMethodResponse(new CallMethod("", new object[] { }, null) { MessageID = callingMessageID }, response, exception as Exception)
                        {
                            MessageID = messageID,
                            CallingMessageID = callingMessageID,
                            Priority = priority
                        };
                        return r;
                    }
                ));
        }

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void ManuallySerialize(Stream stream, object obj)
        {
            var name = obj == null ? "null" : obj.GetType().AssemblyQualifiedName;
            if (!ManualSerilaizationActions.ContainsKey(name))
                throw new InvalidOperationException("Cannot manually serialize this type");
            WriteString(stream, name);
            ManualSerilaizationActions[name].Item1(stream, obj);
        }

        public object ManuallyDeserialize(Stream stream)
        {
            var assemblyQualifiedName = ReadString(stream);
            if (!ManualSerilaizationActions.ContainsKey(assemblyQualifiedName))
                throw new InvalidOperationException("Cannot manually deserialize this type");
            return ManualSerilaizationActions[assemblyQualifiedName].Item2(stream, assemblyQualifiedName);
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
            using (var ms = new MemoryStream())
            {
                methGenericInfo?.Invoke(null, new[] { ms, obj });
                var len = ms.Position;
                WriteShort(stream, (short)len);
                ms.Seek(0, SeekOrigin.Begin);
                if (len > 0) ms.CopyBytesTo(stream, (int)len);
            }
        }

        public object ProtobufDeserialize(Stream stream)
        {
            //TODO: this is very slow. calls should be cashed
            var typeName = ReadString(stream);
            var type = Type.GetType(typeName);
            var len = ReadShort(stream);
            using (var ms = new MemoryStream(len){ Capacity = len })
            {
                if (len > 0) stream.CopyBytesTo(ms, len);
                ms.Seek(0, SeekOrigin.Begin);
                var methodInfo = typeof(Serializer).GetMethods(System.Reflection.BindingFlags.Public |
                                                               System.Reflection.BindingFlags
                                                                   .Static).Where(mi => mi.Name == "Deserialize")
                    .FirstOrDefault(mi => mi.GetParameters().Count() == 2
                                          && mi.GetParameters()[0].ParameterType.Name == "Type" &&
                                          mi.GetParameters()[1].ParameterType.Name == "Stream");

                    return methodInfo?.Invoke(null, new object[] { type, ms });
            }

        }


        public object Deserialize(Stream serializationStream)
        {
            var serializationType = ReadSerializationType(serializationStream);
            switch (serializationType)
            {
                case SerializationType.Manual:
                    return ManuallyDeserialize(serializationStream);

                case SerializationType.Binary:
                    return BinaryDeserialize(serializationStream);

                case SerializationType.Protobuf:
                    return ProtobufDeserialize(serializationStream);
            }

            return null;
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            var type = DetermineApproprateSerialization(graph);
            WriteSerializationType(serializationStream, type);
            switch (type)
            {
                case SerializationType.Manual:
                    ManuallySerialize(serializationStream, graph);
                    break;
                case SerializationType.Binary:
                    BinarySerialize(serializationStream, graph);
                    break;
                case SerializationType.Protobuf:
                    ProtobufSerialize(serializationStream, graph);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


    }
}