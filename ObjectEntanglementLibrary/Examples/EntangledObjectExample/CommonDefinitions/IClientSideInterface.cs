using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonDefinitions
{
    /// <summary>
    /// Simple interface to define calls between client and server
    /// </summary>
    public interface IClientSideInterface
    {
        void WriteToConsole(string message);
        double ReadDoubleFromConsole();
    }
}
