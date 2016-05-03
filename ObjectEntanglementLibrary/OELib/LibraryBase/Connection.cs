#define _DEBUGOUTPUT

using OELib.LibraryBase.Messages;
using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace OELib.LibraryBase
{
    abstract public class Connection
    {
        private ILogger _logger;

        private ByteQuantaClient byteClient;
        private BinaryFormatter bformatter = new BinaryFormatter();
        private System.Timers.Timer _pingTimer = new System.Timers.Timer(60000);
        private AutoResetEvent _pingAutoReset = new AutoResetEvent(false);
        public double PingInterval { get { return _pingTimer.Interval; } set { _pingTimer.Interval = value; } }

        private string _name;
        private TcpClient _tcpClient;

        protected bool started = false;

        public bool IsReady => started && byteClient.IsReady;

        public string Name
        {
            get { return _name; }
            set { _name = value; byteClient.Name = value; }
        }

        public event EventHandler<Message> MessageRecieved;

        private event EventHandler<IControlMessage> controlMessageReceived;

        public event EventHandler<Exception> Stopped;

        public event EventHandler Started;

        public Connection(ILogger logger = null) //TODO: Complete ILog pattern to suit everyone's need
        {
            _logger = logger;
            byteClient = new ByteQuantaClient(this);
            byteClient.PartialDataRead += readActivity;
            byteClient.QuantaReceived += quantaReceived;
            byteClient.Stopped += (s, ex) => Stop(ex);
            _pingTimer.Elapsed += pingTimerElapsed;
            controlMessageReceived += controlMessageIn;
        }

        protected bool Start(TcpClient client)
        {
            _tcpClient = client;

            if (byteClient.Start(_tcpClient))
            {
                _pingTimer.Start();
                Started?.Invoke(this, null);
                started = true;
                return true;
            }
            else return false;
        }

        public void Stop()
        {
            Stop(null);
        }

        protected void Stop(Exception ex)
        {
            if (started)
            {
                started = false;
                _pingTimer.Stop();
                SendMessage(new Bye());//todo send priority message
                byteClient.Stop(ex);
                Stopped?.Invoke(this, ex);
            }
        }

        private void controlMessageIn(object sender, IControlMessage message)
        {
            if (message is Ping)
            {
#if (DEBUGOUTPUT)
                Debug.WriteLine($"{Name} got Ping <- ");
#endif
                SendMessage(new Pong());
            }
            if (message is Pong)
            {
#if (DEBUGOUTPUT)
                Debug.WriteLine($"{Name} got Pong <- ");
#endif
                _pingAutoReset.Set();
            }
            if (message is Bye)
            {
                Stop(new Exception("Graceful goodbye."));
            }
        }

        protected virtual void pingTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _pingTimer.Stop();
            var ping = new Ping();
            _pingAutoReset.Reset();
#if (DEBUGOUTPUT)
            Debug.WriteLine($"{Name} sending Ping -> ");
#endif
            var sendOK = SendMessage(ping);
#if (DEBUGOUTPUT)
            Debug.WriteLine($"{Name } send ping success: {sendOK}");
#endif
            bool ok = _pingAutoReset.WaitOne((int)(PingInterval / 2));
            if (!ok) Stop(new Exception("No ping response"));
            else _pingTimer.Start();
        }

        protected virtual void quantaReceived(object sender, byte[] data)
        {
            if (data.Length == 0) return;
            MemoryStream ms = new MemoryStream();
            ms.Write(data, 0, data.Length);
            ms.Seek(0, SeekOrigin.Begin);
            Message message = null;
            try
            {
                message = bformatter.Deserialize(ms) as Message;
            }
            catch (TargetInvocationException e)
            {
                _logger?.Error($"TargetInvocationException when processing message: {e.ToString()}");
            }

            if (message != null)
            {
                if (message is IControlMessage) controlMessageReceived?.Invoke(this, message as IControlMessage);
                else MessageRecieved?.Invoke(this, message);
            }
        }

        protected virtual void readActivity(object sender, int e)
        {
            _pingAutoReset.Set(); // sometimes there is so much data that ping cant get trough. could get fixed with priority message
            resetPingTimer();
        }

        private void resetPingTimer()
        {
            _pingTimer.Stop();
            _pingTimer.Start();
        }

        public virtual bool SendMessage(Message message)
        {
            if (!byteClient.IsReady) return false;
            BinaryFormatter _bformatter = new BinaryFormatter();
            MemoryStream _ms = new MemoryStream();
            _bformatter.Serialize(_ms, message);
            int length = (int)_ms.Position;
            _ms.Seek(0, SeekOrigin.Begin);
            byte[] buffer = new byte[length];
            _ms.Read(buffer, 0, length);
            return byteClient.SendQuanta(buffer);
        }

        public virtual TraceableMessage Ask(TraceableMessage message, int timeout = 60000)
        {
            //Debug.WriteLine($"{Name} asking {message.ToString()}");
            AutoResetEvent messageReceived = new AutoResetEvent(false);
            TraceableMessage returnMessage = null;
            EventHandler<Message> handler = (sender, msg) =>
            {
                if (msg is TraceableMessage && (msg as TraceableMessage).CallingMessageID == message.MessageID)
                {
                    returnMessage = msg as TraceableMessage;
                    messageReceived.Set();
                }
            };
            MessageRecieved += handler;

            if (!SendMessage(message))
            {
                MessageRecieved -= handler;
                return null;
            }
            else
            {
                messageReceived.WaitOne(timeout);
                MessageRecieved -= handler;
                return returnMessage;
            }
        }

        public virtual async Task<TraceableMessage> AskAsync(TraceableMessage message, int timeout = 60000)
        {
            return await Task<TraceableMessage>.Factory.StartNew(() => { return Ask(message, timeout); });
        }
    }
}