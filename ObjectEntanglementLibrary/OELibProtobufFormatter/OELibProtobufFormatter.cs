using System;
using System.IO;
using System.Runtime.Serialization;


namespace OELibProtobufFormatter
{
    /// <summary>
    /// 
    /// </summary>
    public class OELibProtobufFormatter : IFormatter
    {
        public SerializationHelper SerializationHelper { get; } = new SerializationHelper();


        public object Deserialize(Stream serializationStream)
        {
            var serializationType = SerializationHelper.ReadSerializationType(serializationStream);
            switch (serializationType)
            {
                case SerializationType.Manual:
                    return SerializationHelper.ManuallyDeserialize(serializationStream);

                case SerializationType.Binary:
                    return SerializationHelper.BinaryDeserialize(serializationStream);
                    
                case SerializationType.Protobuf:
                    return SerializationHelper.ProtobufDeserialize(serializationStream);
            }

            return null;
        }

        public void Serialize(Stream serializationStream, object graph)
        {

            var type = SerializationHelper.DetermineApproprateSerialization(graph);
            SerializationHelper.WriteSerializationType(serializationStream, type);
            switch (type)
            {
                case SerializationType.Manual:
                    SerializationHelper.ManuallySerialize(serializationStream, graph);
                    break;
                case SerializationType.Binary:
                    SerializationHelper.BinarySerialize(serializationStream, graph);
                    break;
                case SerializationType.Protobuf:
                    SerializationHelper.ProtobufSerialize(serializationStream, graph);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ISurrogateSelector SurrogateSelector { get; set; }
        public SerializationBinder Binder { get; set; }
        public StreamingContext Context { get; set; }
    }
}