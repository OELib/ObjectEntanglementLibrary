using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectTunnelSpeedTest
{
    [Serializable]
    public class MassivePayload1
    {
        public int[] II { get; set; }
        public double[] DD { get; set; }

        public string S { get; set; }

    }
}
