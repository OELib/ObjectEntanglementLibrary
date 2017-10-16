using System;
using System.Collections.Generic;
using System.Linq;

namespace OELib.PokingConnection
{
    [Serializable]
    public class ObjectInfo
    {
        public List<MethodInfoCommunicationLibrary> Methods { get; protected set; }
        public List<PropertyInfoCommunicationLibrary> Properties { get; protected set; }
        public List<FieldInfoCommunicationLibrary> Fields { get; protected set; }

        public ObjectInfo(object reactingObject)
        {
            Methods = reactingObject.GetType().GetMethods().Select(x => new MethodInfoCommunicationLibrary() { Name = x.Name, IsGeneric = x.IsGenericMethod }).ToList();
            Properties = reactingObject.GetType().GetProperties().Select(x => new PropertyInfoCommunicationLibrary() { Name = x.Name }).ToList();
            Fields = reactingObject.GetType().GetFields().Select(x => new FieldInfoCommunicationLibrary() { Name = x.Name }).ToList();
            //todo: add more if needed
        }
    }

    [Serializable]
    public class MethodInfoCommunicationLibrary
    {
        public string Name { get; set; }

        public bool IsGeneric { get; set; }
    }

    [Serializable]
    public class PropertyInfoCommunicationLibrary
    {
        public string Name { get; set; }
    }

    [Serializable]
    public class FieldInfoCommunicationLibrary
    {
        public string Name { get; set; }
    }
}