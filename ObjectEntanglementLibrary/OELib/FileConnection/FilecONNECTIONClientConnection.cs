using System.Runtime.Serialization;
using OELib.LibraryBase;

namespace OELib.FileConnection
{
    public class FileConnectionClientConnection : ReconnectingClientSideConnection, IFileConnection
    {

        public FileConnectionClientConnection(string rootDir, IFormatter customFormatter = null, ILogger logger = null,
            bool useCompression = false)
            : base(customFormatter, logger, useCompression)
        {
            FileManager = new FileConnectionManager(rootDir);
        }

        public FileConnectionManager FileManager { get; }

    }
}
