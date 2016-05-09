using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleExampleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleServer server = new SimpleServer(8081);

            Console.WriteLine("Server started. Press enter to exit.");


            Console.WriteLine("Press enter to broadcast a message to all connected clients");
            Console.ReadLine();
            server.BroadCastToAllClients();

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }
}
