using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedTests
{
    [Serializable]
    public class BinaryPayloadB
    {
        public byte[] A { get; set; }
        public int[] B { get; set; }
        public double[] C { get; set; }
        public string D { get; set; }
    }
}
