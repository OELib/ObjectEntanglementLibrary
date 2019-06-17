using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OELib.LibraryBase;

namespace OELib.FileConnection
{
    public class ServerFileManager
    {
        private string _rootDir;

        public ServerFileManager(string rootDir)
        {
            _rootDir = rootDir;
        }

        public void HandleRequest(FileInfoRequest request, Connection client)
        {
            switch (request)
            {
                case FileListingRequest fir:
                    break;
                case FileGetRequest fgr:
                    break;
            }

        }



    }
}
