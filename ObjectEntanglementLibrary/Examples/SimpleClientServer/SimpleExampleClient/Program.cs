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

            bool running = true;
            while (running)
            {
                Console.WriteLine(
$@"Client started. Select an action:
Q:  Quit
P:  Send a Ping and wait for a Pong reply.
L:  Send a large object and wait for the server to return it.
V:  Call a void method (fire and forget).");

                ConsoleKeyInfo keyPressed = Console.ReadKey(true);

                switch (keyPressed.Key)
                {
                    case ConsoleKey.Q:
                        running = false;
                        break;
                    case ConsoleKey.P:
                        client.SendPing();
                        break;
                    case ConsoleKey.L:
                        client.SendLargeObject();
                        break;
                    case ConsoleKey.V:
                        client.CallVoidMethodFireAndForget();
                        break;
                    default:
                        break;
                }
            }

            Console.WriteLine("Exiting...");
        }
    }
}
