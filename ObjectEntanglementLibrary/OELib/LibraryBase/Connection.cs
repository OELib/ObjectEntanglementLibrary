#define _DEBUGOUTPUT

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using OELib.LibraryBase.Messages;
using Timer = System.Timers.Timer;

namespace OELib.LibraryBase
{
    public abstract class Connection
    {
        private readonly ByteQuantaClient _byteClient;

        public IFormatter Formatter { get; set; }
        private readonly ILogger _logger;
        private readonly AutoResetEvent _pingAutoReset = new AutoResetEvent(false);

        private readonly Timer _pingTimer = new Timer(60000);

        private string _name;

        protected bool _started;
        private TcpClient _tcpClient;

        protected Connection(IFormatter serializer = null, ILogger logger = null,
            bool useCompression = false) //TODO: Complete ILog pattern to suit everyone's need
        {
            Formatter = serializer ?? new BinaryFormatter();
            _logger = logger;
            _byteClient = new ByteQuantaClient(this, useCompression);
            _byteClient.PartialDataRead += readActivity;
            _byteClient.QuantaReceived += quantaReceived;
            _byteClient.Stopped += (s, ex) => Stop(ex);
            _pingTimer.Elapsed += pingTimerElapsed;
            controlMessageReceived += controlMessageIn;
        }

        public double PingInterval
        {
            get => _pingTimer.Interval;
            set => _pingTimer.Interval = value;
        }

        public bool IsReady => _started && _byteClient.IsReady;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                _byteClient.Name = value;
            }
        }

        public event EventHandler<Message> MessageRecieved;

        private event EventHandler<IControlMessage> controlMessageReceived;

        public event EventHandler<Exception> Stopped;

        public event EventHandler Started;

        // ReSharper disable once InconsistentNaming
        protected bool Start(TcpClient client)
        {
            _tcpClient = client;

            if (_byteClient.Start(_tcpClient))
            {
                _pingTimer.Start();
                Started?.Invoke(this, null);
                _started = true;
                return true;
            }
            return false;
        }

        public void Stop()
        {
            Stop(null);
        }

        // ReSharper disable once InconsistentNaming
        protected void Stop(Exception ex)
        {
            if (_started)
            {
                _started = false;
                _pingTimer.Stop();
                SendMessage(new Bye());
                _byteClient.Stop(ex);
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
                Stop(new Exception($" {Name} Graceful goodbye."));
        }

        protected virtual void pingTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _pingTimer.Stop();
            var ping = new Ping();
            _pingAutoReset.Reset();
#if (DEBUGOUTPUT)
            Debug.WriteLine($"{Name} sending Ping -> ");
#endif
            // ReSharper disable once UnusedVariable
            var sendOK = SendMessage(ping);
#if (DEBUGOUTPUT)
            Debug.WriteLine($"{Name } send ping success: {sendOK}");
#endif
            var ok = _pingAutoReset.WaitOne((int) (PingInterval / 2));
            if (!ok) Stop(new Exception("No ping response"));
            else _pingTimer.Start();
        }

        protected virtual void quantaReceived(object sender, byte[] data)
        {
            if (data.Length == 0) return;

            var ms = new MemoryStream();
            ms.Write(data, 0, data.Length);
            ms.Seek(0, SeekOrigin.Begin);
            Message message = null;
            try
            {
#if (DEBUGOUTPUT)
                Debug.WriteLine($"Client {Name} connection got a message with formatter {Formatter.ToString()}.");
#endif
                message = Formatter.Deserialize(ms) as Message;
            }
            catch (TargetInvocationException e)
            {
                _logger?.Error($"TargetInvocationException when processing message: {e}");
            }
#if (DEBUGOUTPUT)
            catch (Exception ex)
            {
                Debug.WriteLine($"Client {Name} deserialization error {ex}");
            }
#endif
            if (message != null)
                if (message is IControlMessage) controlMessageReceived?.Invoke(this, message as IControlMessage);
                else MessageRecieved?.Invoke(this, message);
        }

        protected virtual void readActivity(object sender, int e)
        {
            _pingAutoReset
                .Set(); // sometimes there is so much data that ping cant get trough. could get fixed with priority message
            resetPingTimer();
        }

        private void resetPingTimer()
        {
            _pingTimer.Stop();
            _pingTimer.Start();
        }

        public virtual bool SendMessage(Message message)
        {
            if (!_byteClient.IsReady) return false;
            var ms = new MemoryStream();
#if (DEBUGOUTPUT)
            Debug.WriteLine($"Client {Name} connection sending message {message.ToString()} with formatter {Formatter.ToString()}.");
#endif
            Formatter.Serialize(ms, message);
            var length = (int) ms.Position;
            ms.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[length];
            ms.Read(buffer, 0, length);
            return _byteClient.SendQuanta(buffer, message.Priority);
        }

        public virtual TraceableMessage Ask(TraceableMessage message, int timeout = 60000)
        {
            //Debug.WriteLine($"{Name} asking {message.ToString()}");
            var messageReceived = new AutoResetEvent(false);
            TraceableMessage returnMessage = null;
            // ReSharper disable once ConvertToLocalFunction
            EventHandler<Message> handler = (sender, msg) =>
            {
                // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
                if (msg is TraceableMessage && ((TraceableMessage) msg).CallingMessageID == message.MessageID)
                {
                    returnMessage = (TraceableMessage) msg;
                    messageReceived.Set();
                }
            };
            MessageRecieved += handler;

            if (!SendMessage(message))
            {
                MessageRecieved -= handler;
                return null;
            }
            messageReceived.WaitOne(timeout);
            MessageRecieved -= handler;
            return returnMessage;
        }

        public virtual async Task<TraceableMessage> AskAsync(TraceableMessage message, int timeout = 60000)
        {
            return await Task<TraceableMessage>.Factory.StartNew(() => Ask(message, timeout));
        }
    }
}