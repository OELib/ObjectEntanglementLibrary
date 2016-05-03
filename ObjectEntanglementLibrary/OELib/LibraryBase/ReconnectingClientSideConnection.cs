using System;
using System.Timers;

namespace OELib.LibraryBase
{
    public class ReconnectingClientSideConnection : ClientSideConnection
    {
        private Timer _reconnectTimer = new Timer(500);

        public event EventHandler Restarting;

        public bool EnableRestart { get; set; } = true;

        public double ReconnectionInterval { get { return _reconnectTimer.Interval; } set { _reconnectTimer.Interval = value; } }

        public ReconnectingClientSideConnection()
            : base()
        {
            _reconnectTimer.Elapsed += _reconnectTimer_Elapsed;
            Stopped += StartReconnectingTimer;
        }

        private void StartReconnectingTimer(object sender, Exception e)
        {
            if (e != null)
                _reconnectTimer.Start();
        }

        private void _reconnectTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _reconnectTimer.Stop();
            if (!EnableRestart) return;
            Restarting?.Invoke(this, null);
            Start(remoteIP, remotePort);
        }

        public override bool Start(string IpAddress, int port)
        {
            bool ok = base.Start(IpAddress, port);
            if (!ok) StartReconnectingTimer(this, new Exception("Start failed"));
            return ok;
        }
    }
}