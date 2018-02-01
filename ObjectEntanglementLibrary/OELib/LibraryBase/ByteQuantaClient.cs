#define DEBUGOUTPUT

using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using OELib.LibraryBase.Messages;

namespace OELib.LibraryBase
{
    /// <summary>
    ///     wraps TCP client and sends/receives byte packets with packet size in a simple header.
    /// </summary>
    public class ByteQuantaClient
    {
        private const int _zeroReadLimit = 10;
        private readonly Connection _parent;
        private readonly Actor _rcvActor = new Actor();
        private readonly Actor _sendActor = new Actor();
        private bool _disconnectEventSent = true;
        protected int _maxChunkSize = 10240;

        private bool _reading;
        protected Thread _readThread;

        public ByteQuantaClient(Connection parent, bool useCompression = false)
        {
            _parent = parent;
            UseCompression = useCompression;
        }

        public string Name { get; set; }

        public bool UseCompression { get; }

        public bool IsReady => _reading && Client.Client != null && Client.Connected;

        public TcpClient Client { get; protected set; }

        public event EventHandler<int> PartialDataRead;

        public event EventHandler<byte[]> QuantaReceived;

        public event EventHandler<Exception> Stopped;

        public event EventHandler Started;

        public bool Start(TcpClient client)
        {
            if (client.Connected && !_reading)
            {
                Client = client;
                beginReadLoop();
                _disconnectEventSent = false;
                Started?.Invoke(this, null);
                return true;
            }
            return false;
        }

        private void beginReadLoop()
        {
            if (_reading) return;
            _reading = true;
            _readThread = new Thread(() =>
                {
                    while (_reading)
                        try
                        {
                            var headerSize = MessageHeader.HeaderSize;

                            var headerBuffer = new byte[headerSize];
                            var read = 0;
                            do
                            {
                                try
                                {
                                    read = Client.Client.Receive(headerBuffer, headerSize, SocketFlags.None);
                                    if (read == 0)
                                        throw new Exception("Error reading header. (Probably a graceful shutdown.)");
                                    if (read != headerSize)
                                        throw new Exception(
                                            $"Error reading header. (Not enough data in header - required {headerSize} bytes, got {read} bytes.)");
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
                            } while (_reading && read < headerSize);

                            var messageHeader = StructMarshaller.FromByteArray<MessageHeader>(headerBuffer);
                            var packageSize = messageHeader.Length;
#if (DEBUGOUTPUT)
                        Debug.WriteLine($"Getting package size {packageSize}");
#endif
                            var zeroReadCount = 0;
                            var data = new byte[packageSize];
                            var position = 0;
                            while (position < packageSize && _reading)
                            {
                                var toRead = Client.Client.Available;
                                if (toRead + position > packageSize) toRead = packageSize - position;
                                if (toRead > _maxChunkSize) toRead = _maxChunkSize;
                                read = Client.Client.Receive(data, position, toRead, SocketFlags.None);

                                if (read == 0)
                                    zeroReadCount
                                        ++; //todo: this should be time based not count. first time you get zero bytes, if you don't get anything within ~1 second, the connection is dead.
                                else
                                    zeroReadCount = 0;
                                if (zeroReadCount > _zeroReadLimit)
                                    throw new Exception(
                                        $"Error reading data in '{_parent?.Name}'. (Probably a graceful shutdown or zero read limit reached)");
                                position += read;
                                PartialDataRead?.Invoke(this, read);
                            }

                            var uncompressedData = messageHeader.DataIsCompressed ? GZipper.Unzip(data) : data;

                            _rcvActor.Post(() => QuantaReceived?.Invoke(this, uncompressedData));
                        }
                        catch (Exception ex)
                        {
#if (DEBUGOUTPUT)
                        Debug.WriteLine($"Stopping the byte quanta client because {ex.ToString()}");
#endif
                            Stop(ex);
                            return;
                        }
                    Stop();
                })
                {IsBackground = true, Name = "Byte client reading thread"};
            _readThread.Start();
        }

        public bool SendQuanta(byte[] data, Priority priority = Priority.Normal)
        {
            if (!IsReady) return false;
            var ok = false;
            var ok2 = _sendActor.PostWait(() =>
            {
                var compressedPacket = UseCompression ? GZipper.Zip(data) : data;

                var length = compressedPacket.Length;

                var header =
                    StructMarshaller.ToByteArray(
                        new MessageHeader {Length = length, DataIsCompressed = UseCompression});
                var packet = new byte[length + header.Length];

                Array.Copy(header, 0, packet, 0, header.Length);
                Array.Copy(compressedPacket, 0, packet, header.Length, length);
                var sent = 0;
                try
                {
                    sent = Client.Client.Send(packet, length + header.Length, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    Stop(ex);
                }
                ok = sent == length + header.Length;
            }, priority);
            return ok && ok2;
        }

        public void Stop(Exception ex = null)
        {
            _reading = false;
            if (Client.Client != null && Client.Connected) Client.Close();
            try
            {
                _readThread.Abort();
            }
            catch (ThreadAbortException)
            {
            }
            if (!_disconnectEventSent)
            {
                _disconnectEventSent = true;
                Stopped?.Invoke(this, ex);
            }
        }

        public override string ToString()
        {
            return $"Byte quanta client {Name}";
        }
    }
}