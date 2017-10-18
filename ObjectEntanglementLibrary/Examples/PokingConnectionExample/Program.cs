using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using ProtoBuf;


namespace PokingConnectionExample
{
    class Program
    {
        // ReSharper disable once InconsistentNaming
        static void Main_(string[] args)
        {

            var payloads = Enumerable.Range(0, 100).Select(i => Payload.Generate()).ToList();
            var s = new Server();
            // ReSharper disable once ObjectCreationAsStatement
            new Client();
            var sw = Stopwatch.StartNew();
            payloads.ForEach(pl => s.SendPayload(pl));
            sw.Stop();
            Console.WriteLine($"Payload transport took {sw.ElapsedMilliseconds} ms.");
            Console.ReadKey();
        }

        static void Main(string[] args)

        {

            var payloads = Enumerable.Range(0, 100).Select(i => Payload.Generate()).ToList();
            using(var file =File.Create("payloads.bin"))
                ProtoBuf.Serializer.Serialize<List<Payload>>( file, payloads);
            List<Payload> desPlds;
            using (var file = File.OpenRead("payloads.bin"))
            {
                desPlds = Serializer.Deserialize<List<Payload>>(file);
            }

        }


    }
}
