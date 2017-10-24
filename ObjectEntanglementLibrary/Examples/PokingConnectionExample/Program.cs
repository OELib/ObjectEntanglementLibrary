using System;
using System.Linq;
using System.Diagnostics;


namespace PokingConnectionExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var payloads = Enumerable.Range(0, 5).Select(i => Payload.Generate()).ToList();
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
