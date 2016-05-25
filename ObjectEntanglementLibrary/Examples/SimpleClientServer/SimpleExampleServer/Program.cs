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

            bool running = true;
            while (running)
            {
                Console.WriteLine(
$@"Server started. Select an action:
Q:  Quit
B:  Broadcast a message to all connected clients");

                ConsoleKeyInfo keyPressed = Console.ReadKey(true);

                switch (keyPressed.Key)
                {
                    case ConsoleKey.Q:
                        running = false;
                        break;
                    case ConsoleKey.B:
                        server.BroadCastToAllClients();
                        break;
                    default:
                        break;
                }
            }

            Console.WriteLine("Exiting...");
        }
    }
}
