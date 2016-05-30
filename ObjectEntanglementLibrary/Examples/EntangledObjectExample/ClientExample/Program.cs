﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientExample
{
    class Program
    {
        public class Reactor : CommonDefinitions.IClientSideInterface
        {
            public double ReadDoubleFromConsole()
            {
                return Convert.ToDouble(Console.ReadLine());
            }

            public void WriteToConsole(string message)
            {
                Console.WriteLine(message);
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hit enter to connect");
            Console.ReadLine();
            var client = new OELib.ObjectEntanglement.EntangledClientConnection<CommonDefinitions.IServerSideInterface>(new Reactor());
            client.Start("127.0.0.1", 8888);
            Console.WriteLine("Press enter to say hello to server");
            Console.ReadLine();
            client.RemoteEntangledObject.Echo("Hello");
            Console.WriteLine("Waiting 2 minutes before exit");

            Thread.Sleep(2 * 60 * 1000);


        }
    }
}
