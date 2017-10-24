using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OELib.LibraryBase;
using OELib.PokingConnection;

namespace SpeedTests
{
    class Program
    {


        public static void GenerateRandomData(out BinaryPayloadA bpa, out BinaryPayloadB bpb, out ProtobufPayloadA ppa, out ProtobufPayloadB ppb)
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


        static void Main(string[] args)
        {

           
            TestFormatter(null, null, ".Net BinaryFormatter", 1024, "Method1");
            Console.WriteLine("---------------------------------------------------------------------------");
            
            var serverFormatter = new OELibProtobufFormatter.OELibProtobufFormatter();
            var clientFormatter = new OELibProtobufFormatter.OELibProtobufFormatter();
            TestFormatter(serverFormatter, clientFormatter, "Manual Binary hybrid", 1025, "Method1");
            Console.WriteLine("---------------------------------------------------------------------------");
            
            TestFormatter2(serverFormatter, clientFormatter, "Manual Binary Protobuf hybrid", 1026, "Method2");

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        private static void TestFormatter(OELibProtobufFormatter.OELibProtobufFormatter serverFormatter, OELibProtobufFormatter.OELibProtobufFormatter clientFormatter,
            string formatterName, int port, string MethodName)
        {
            var sro = new ReactingObject();
            var go = new AutoResetEvent(false);
            var server =
                new PokingServer(port, sro, serverFormatter, null, false);
            server.Start();
            server.ClientConnected += (s, e) => 
            {
                //Console.WriteLine($"Client connected to the server, server formatter is {server.Formatter}.");
                go.Set();
                //e.PingInterval = 10000000;
                e.MessageRecieved += (se, me) =>
                {
                    //Console.WriteLine($"Server got message {me.ToString()}.");
                };
            };

            var cro = new ReactingObject();
            var client = new PokingClientConnection(cro, clientFormatter, null, false) { /*PingInterval = 1000000 */};
            client.MessageRecieved += (se, me) =>
            {
                //Console.WriteLine($"Client got a message {me.ToString()}.");
            };
            
            
            client.Start("127.0.0.1", port);
            var ok = go.WaitOne(500);
            if (ok)
            {
                var sw = Stopwatch.StartNew();
                for (int itteration = 0; itteration < 100; itteration++)
                {
                    var a = client.Reactor.CallRemoteMethod(MethodName, cro.bpa);
                    a = a;
                }
                sw.Stop();
                Console.WriteLine($"{formatterName} serialization took {sw.ElapsedMilliseconds} ms to complete info transfer.");
            }
            else

            {
                Console.WriteLine("Could not connect.");
            }
        }


        private static void TestFormatter2(OELibProtobufFormatter.OELibProtobufFormatter serverFormatter, OELibProtobufFormatter.OELibProtobufFormatter clientFormatter,
            string formatterName, int port, string MethodName)
        {
            var sro = new ReactingObject();
            var go = new AutoResetEvent(false);
            var server =
                new PokingServer(port, sro, serverFormatter) {};
            server.Start();
            server.ClientConnected += (s, e) => 
            {
                go.Set();
                e.PingInterval = 100000;
            };

            var cro = new ReactingObject();
            var client = new PokingClientConnection(cro, clientFormatter) {PingInterval = 1000000};

            client.Start("127.0.0.1", port);
            Thread.Sleep(100);
            var ok = go.WaitOne(500);
            if (ok)
            {
                var sw = Stopwatch.StartNew();
                for (int itteration = 0; itteration < 100; itteration++)
                {
                    var a = client.Reactor.CallRemoteMethod(MethodName, cro.ppa);
                    a = a;
                }
                sw.Stop();
                Console.WriteLine($"{formatterName} serialization took {sw.ElapsedMilliseconds} ms to complete info transfer.");
            }
            else

            {
                Console.WriteLine("Could not connect.");
            }
        }


    }
}
