using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace SpeedTests
{
    [ProtoContract]
    public class ProtobufPayloadB
    {
        [ProtoMember(1)]
        public byte[] A { get; set; }
        [ProtoMember(2)]
        public int[] B { get; set; }
        [ProtoMember(3)]
        public double[] C { get; set; }
        [ProtoMember(4)]
        public string D { get; set; }
    }
}
