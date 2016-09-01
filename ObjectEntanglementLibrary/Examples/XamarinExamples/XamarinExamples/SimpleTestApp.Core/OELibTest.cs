using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTestApp.Core
{
    public class OELibTest
    {
        public OELib.PokingConnection.PokingClientConnection Client { get; set; }

        public event EventHandler ClientConnected;

        public event EventHandler<string> ReceivedChatString;

        public void Connect(string serverIp, int serverPort)
        {
            Client = new OELib.PokingConnection.PokingClientConnection(this);
            Client.Start(serverIp, serverPort);
            Console.WriteLine("Trying to connect...");
            Client.Reactor.RemoteReactingInspectionComplete += (s, e) =>
            {
                Console.WriteLine("Connected!");
                ClientConnected?.Invoke(s, null);
            };
        }


        public TestObject GetTestObject() => (TestObject)Client.Reactor.CallRemoteMethod("GetTestObject");

        public void HandleChatString(string receivedChatString)
        {
            ReceivedChatString?.Invoke(this, receivedChatString);
        }

        public void SendChatMessageToServer(string text)
        {
            Client.Reactor.CallRemoteVoidMethod(OELib.LibraryBase.Priority.Normal, "HandleChatMessage", text);
        }
    }
}
