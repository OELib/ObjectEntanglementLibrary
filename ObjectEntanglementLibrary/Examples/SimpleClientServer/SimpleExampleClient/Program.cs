using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleExampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleClient client = new SimpleClient("127.0.0.1", 8081);

            Console.WriteLine("Client started. Press enter to send a Ping and wait for a Pong reply");
            Console.ReadLine();
            client.SendPing();

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
    }
}
