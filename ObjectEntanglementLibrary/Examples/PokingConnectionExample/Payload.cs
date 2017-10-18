using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace PokingConnectionExample
{
    [ProtoContract]
    [Serializable]
    public class Payload
    {
        [ProtoMember(1)]
        public string A { get; set; }
        [ProtoMember(2)]
        public int B { get; set; }
        [ProtoMember(3)]
        public double C { get; set; }
        [ProtoMember(4)]
        public List<Tuple<string, int, double>> D { get; set; }
        [ProtoMember(5)]
        public double[] E { get; set; }


        public static Payload Generate()
        {
            var rnd = new Random();
            return new Payload()
            {
                A = Guid.NewGuid().ToString(),
                B = (int)(rnd.NextDouble() * 100),
                C = rnd.NextDouble(),
                D = Enumerable.Range(0, 10000).Select(i=>
                new Tuple<string, int, double>(Guid.NewGuid().ToString(), (int)rnd.NextDouble()*100, rnd.NextDouble())).ToList(),
                E = Enumerable.Range(0, 10000).Select(i=>rnd.NextDouble()).ToArray()
            };
        }

    }
}
