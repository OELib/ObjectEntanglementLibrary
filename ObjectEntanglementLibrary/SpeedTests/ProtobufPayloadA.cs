using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace SpeedTests
{
    [ProtoContract]
    public class ProtobufPayloadA
    {
        [ProtoMember(1)]
        public double A { get; set; }
        [ProtoMember(2)]
        public double B { get; set; }
        [ProtoMember(3)]
        public double C { get; set; }
        [ProtoMember(4)]
        public double D { get; set; }
        [ProtoMember(5)]
        public double E { get; set; }
        [ProtoMember(6)]
        public double F { get; set; }
        [ProtoMember(7)]
        public double G { get; set; }
        [ProtoMember(8)]
        public double[] H { get; set; }
        [ProtoMember(9)]
        public string I { get; set; }

    }
}
