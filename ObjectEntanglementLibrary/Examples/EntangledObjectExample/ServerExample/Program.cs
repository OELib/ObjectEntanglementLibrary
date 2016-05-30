using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerExample
{
    class Program
    {
        public class ServerReactingObject : CommonDefinitions.IServerSideInterface
        {
            public void Echo(string message)
            {
                Console.WriteLine(message);
            }            
        }
        
        static void Main(string[] args)
        {
            Console.WriteLine("Press enter to start server");
            Console.ReadLine();
            var server = new OELib.ObjectEntanglement.EntangledServer<CommonDefinitions.IClientSideInterface>(8888, new ServerReactingObject());
            server.Start();

            Console.WriteLine("Hit enter to exit");
            Console.ReadLine();
        }
    }
}
