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
            return SerializationHelper.Deserialize(serializationStream);
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            SerializationHelper.Serialize(serializationStream, graph);
        }

        public ISurrogateSelector SurrogateSelector { get; set; }
        public SerializationBinder Binder { get; set; }
        public StreamingContext Context { get; set; }
    }
}