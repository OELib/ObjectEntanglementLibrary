using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedTests
{
    public class ReactingObject
    {
        public BinaryPayloadA bpa;
        public BinaryPayloadB bpb;
        public ProtobufPayloadA ppa;
        public ProtobufPayloadB ppb;

        public ReactingObject()
        {
            bpa = new BinaryPayloadA()
            {
                A = 1,
                B = 2,
                C = 3,
                D = 4,
                E = 5,
                F = 6,
                G = 7,
                H = Enumerable.Range(0, 100).Select(i => (double)i).ToArray(),
                I = "Test string"
            };

            ppa = new ProtobufPayloadA()
            {
                A = 1,
                B = 2,
                C = 3,
                D = 4,
                E = 5,
                F = 6,
                G = 7,
                H = Enumerable.Range(0, 100).Select(i => (double)i).ToArray(),
                I = "Test string"
            };

            bpb = new BinaryPayloadB()
            {
                A = Enumerable.Range(0, 100).Select(i => (byte)i).ToArray(),
                B = Enumerable.Range(0, 100).Select(i => i).ToArray(),
                C = Enumerable.Range(0, 100).Select(i => (double)i).ToArray(),
                D = "Some text"
            };

            ppb = new ProtobufPayloadB()
            {
                A = Enumerable.Range(0, 100).Select(i => (byte)i).ToArray(),
                B = Enumerable.Range(0, 100).Select(i => i).ToArray(),
                C = Enumerable.Range(0, 100).Select(i => (double)i).ToArray(),
                D = "Some text"
            };
        }


        public BinaryPayloadB Method1(BinaryPayloadA bpa)
        {
            return bpb;
        }

        public ProtobufPayloadB Method2(ProtobufPayloadA ppa)
        {
            return ppb;
        }
        
    }
}
