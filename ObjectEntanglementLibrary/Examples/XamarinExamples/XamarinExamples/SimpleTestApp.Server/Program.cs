using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTestApp.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleServer server = new SimpleServer(8081);

            string chatStringToSend = "";
            Console.WriteLine(
$@"Server started. Select an action:
Q:  Quit
Type text to chat with clients. Press enter to send message. (Avoid using 'Q' when chatting)");
            bool running = true;
            while (running)
            {


                ConsoleKeyInfo keyPressed = Console.ReadKey();

                switch (keyPressed.Key)
                {
                    case ConsoleKey.Q:
                        running = false;
                        break;
                    case ConsoleKey.Enter:
                        server.SendChatTextToAllClients(chatStringToSend);
                        Console.WriteLine($"Server: {chatStringToSend}");
                        chatStringToSend = "";
                        break;
                    default:
                        chatStringToSend += keyPressed.KeyChar;
                        break;
                }
            }

            Console.WriteLine("Exiting...");
        }
    }
}
