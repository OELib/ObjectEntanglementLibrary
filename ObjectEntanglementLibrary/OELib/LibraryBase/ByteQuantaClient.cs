#define _DEBUGOUTPUT

using System;
using System.Net.Sockets;
using System.Threading;

namespace OELib.LibraryBase
{
    /// <summary>
    /// wraps TCP client and sends/receives byte packets with packet size in a simple header.
    /// </summary>
    public class ByteQuantaClient
    {
        public string Name { get; set; }

        public event EventHandler<int> PartialDataRead;

        public event EventHandler<byte[]> QuantaReceived;

        public event EventHandler<Exception> Stopped;

        public event EventHandler Started;

        public bool IsReady => _reading && Client.Client != null && Client.Connected;

        public TcpClient Client { get; protected set; }

        private bool _reading;
        private Actor sendActor = new Actor();
        private Actor rcvActor = new Actor();
        protected int maxChunkSize = 10240;
        protected Thread readThread;
        private Connection _parent;
        private bool _disconnectEventSent = true;
        private int _zeroReadLimit = 10;

        public ByteQuantaClient(Connection parent)
        {
            _parent = parent;
        }

        public bool Start(TcpClient client)
        {
            if (client.Connected && !_reading)
            {
                Client = client;
                BeginReadLoop();
                _disconnectEventSent = false;
                Started?.Invoke(this, null);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void BeginReadLoop()
        {
            if (_reading) return;
            _reading = true;
            readThread = new Thread(() =>
            {
                while (_reading)
                {
                    try
                    {
                        int packageSize;
                        byte[] sizeBuffer = new byte[4];
                        int read = 0;
                        do
                        {
                            try
                            {
                                read = Client.Client.Receive(sizeBuffer, 4, SocketFlags.None);
                                if (read == 0) throw new Exception("Error reading header. (Probably a graceful shutdown.)");
                                if (read != 4) throw new Exception($"Error reading header. (Not enough data in header - required 4 bytes, got {read} bytes.)");
#if (DEBUGOUTPUT)
                                Debug.WriteLine($"Read header {read}");
#endif
                                PartialDataRead?.Invoke(this, read);
                            }
                            catch (SocketException sex) when (sex.SocketErrorCode == SocketError.TimedOut)
                            {
#if (DEBUGOUTPUT)
                                Debug.WriteLine("Waiting for header.");
#endif
                            }
                        } while (_reading && read < 4);
                        packageSize = BitConverter.ToInt32(sizeBuffer, 0);
#if (DEBUGOUTPUT)
                        Debug.WriteLine($"Getting package size {packageSize}");
#endif
                        int zeroReadCount = 0;
                        byte[] data = new byte[packageSize];
                        int position = 0, toRead = 0;
                        while (position < packageSize && _reading)
                        {
                            toRead = Client.Client.Available;
                            if (toRead + position > packageSize) toRead = packageSize - position;
                            if (toRead > maxChunkSize) toRead = maxChunkSize;
                            read = 0;
                            read = Client.Client.Receive(data, position, toRead, SocketFlags.None);

                            if (read == 0)
                                zeroReadCount++; //todo: this should be time based not count. first time you get zero bytes, if you don't get anything within ~1 second, the connection is dead.
                            else
                                zeroReadCount = 0;
                            if (zeroReadCount > _zeroReadLimit)
                                throw new Exception($"Error reading data in '{_parent?.Name}'. (Probably a graceful shutdown or zero read limit reached)");
                            position += read;
                            PartialDataRead?.Invoke(this, read);
                        }
                        rcvActor.Post(() => QuantaReceived?.Invoke(this, data));
                    }
                    catch (Exception ex)
                    {
#if (DEBUGOUTPUT)
                        Debug.WriteLine($"Stopping the byte quanta client because {ex.ToString()}");
#endif
                        Stop(ex);
                        return;
                    }
                }
                Stop(null);
            })
            { IsBackground = true, Name = "Byte client reading thread" };
            readThread.Start();
        }

        public bool SendQuanta(byte[] data)
        {
            if (!IsReady) return false;
            bool ok = false;
            bool ok2 = sendActor.PostWait(() =>
            {
                int length = data.Length;
                byte[] header = BitConverter.GetBytes(length);
                byte[] packet = new byte[length + 4];
                Array.Copy(header, 0, packet, 0, 4);
                Array.Copy(data, 0, packet, 4, length);
                int sent = 0;
                try
                {
                    sent = Client.Client.Send(packet, length + 4, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    Stop(ex);
                }
                ok = (sent == length + 4);
            });
            return (ok && ok2);
        }

        public void Stop(Exception ex = null)
        {
            _reading = false;
            if (Client.Client != null && Client.Connected) Client.Close();
            try
            {
                readThread.Abort();
            }
            catch (ThreadAbortException) { }
            if (!_disconnectEventSent)
            {
                _disconnectEventSent = true;
                Stopped?.Invoke(this, ex);
            }
        }

        public override string ToString() => $"Byte quanta client {Name}";
    }
}