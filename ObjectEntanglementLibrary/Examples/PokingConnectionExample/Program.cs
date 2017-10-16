using System;
using System.Linq;
using System.Diagnostics;

namespace PokingConnectionExample
{
    class Program
    {
        // ReSharper disable once InconsistentNaming
        static void Main(string[] args)
        {

            var payloads = Enumerable.Range(0, 10000).Select(i => new Payload()).ToList();
            var s = new Server();
            // ReSharper disable once ObjectCreationAsStatement
            new Client();
            
            var sw = Stopwatch.StartNew();
            payloads.ForEach(pl => s.SendPayload(pl));
            sw.Stop();
            Console.WriteLine($"Payload transport took {sw.ElapsedMilliseconds} ms.");
            Console.ReadKey();
        }
    }
}
