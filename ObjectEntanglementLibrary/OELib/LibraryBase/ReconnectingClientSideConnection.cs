using System;
using System.Runtime.Serialization;
using System.Timers;

namespace OELib.LibraryBase
{
    public class ReconnectingClientSideConnection : ClientSideConnection
    {
        private readonly Timer _reconnectTimer = new Timer(500);

        public ReconnectingClientSideConnection(IFormatter customFormatter = null, ILogger logger = null, bool useCompression = false)
            : base(customFormatter, logger, useCompression)
        {
            _reconnectTimer.Elapsed += _reconnectTimer_Elapsed;
            Stopped += startReconnectingTimer;
        }

        public bool EnableRestart { get; set; } = true;

        public double ReconnectionInterval
        {
            get => _reconnectTimer.Interval;
            set => _reconnectTimer.Interval = value;
        }

        public event EventHandler Restarting;

        private void startReconnectingTimer(object sender, Exception e)
        {
            if (e != null)
                _reconnectTimer.Start();
        }

        private void _reconnectTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _reconnectTimer.Stop();
            if (!EnableRestart) return;
            Restarting?.Invoke(this, null);
            Start(_remoteIp, _remotePort);
        }

        public override bool Start(string ipAddress, int port)
        {
            var ok = base.Start(ipAddress, port);
            if (!ok) startReconnectingTimer(this, new Exception("Start failed"));
            return ok;
        }
    }
}