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
        private readonly SerializationHelper _serializationHelper = new SerializationHelper();


        public object Deserialize(Stream serializationStream)
        {
            var serializationType = _serializationHelper.ReadSerializationType(serializationStream);
            switch (serializationType)
            {
                case SerializationType.Manual:
                    return _serializationHelper.ManuallyDeserialize(serializationStream);

                case SerializationType.Binary:
                    break;
                case SerializationType.Protobuf:
                    break;
                
                    
            }

            return null;
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            switch (_serializationHelper.DetermineApproprateSerialization(graph))
            {
                case SerializationType.Manual:
                    _serializationHelper.ManuallySerialize(serializationStream, graph);
                    break;
                case SerializationType.Binary:
                    break;
                case SerializationType.Protobuf:
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