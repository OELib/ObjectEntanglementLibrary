using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleExampleCommon
{
    [Serializable]
    public class Pong
    {
        public byte[] Data { get; set; } = new byte[100000000];
    }
}
