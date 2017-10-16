using System;
using System.Collections.Generic;
using System.Linq;

namespace PokingConnectionExample
{
    [Serializable]
    public class Payload
    {
        public string A { get; set; }
        public int B { get; set; }
        public double C { get; set; }
        public List<Tuple<string, int, double>> D { get; set; }
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
