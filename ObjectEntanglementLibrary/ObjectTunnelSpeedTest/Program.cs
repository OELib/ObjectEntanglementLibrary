using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using OELib.ObjectTunnel;


namespace ObjectTunnelSpeedTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var tp1 = new MassivePayload1()
            {
                DD = Enumerable.Range(0, 5000).Select(i => (double) i).ToArray(),
                II = Enumerable.Range(0, 5000).Select(i => i).ToArray(),
                S = Enumerable.Range(0, 5000).Select(i => i.ToString()).Aggregate((f, s) => f + " - " + s)
            };
            Console.WriteLine($"Sending 200 messages took {MeasureTime(1024, tp1, 200)} ms.");
            Console.ReadLine();
        }


        public static double MeasureTime(int port, object msg, int repetitions)
        {
            var server = new ObjectTunnelServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1044));
            server.Start();
            var client = new ObjectTunnelClientConnection();
            var go1 = new AutoResetEvent(false);
            var go2 = new AutoResetEvent(false);
            client.Started += (s, e) =>
            {
                go1.Set();
            };
            client.Start("127.0.0.1", 1044);
            go1.WaitOne(1000);
            List<object> recvd = new List<object>();
            client.ObjectReceived += (s, o) =>
            {

                recvd.Add(o);
                if (recvd.Count == repetitions-1)
                    go2.Set();
            };
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < repetitions; i++)
                server.SendObject(msg);
            go2.WaitOne(100000);
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }


    }
}
