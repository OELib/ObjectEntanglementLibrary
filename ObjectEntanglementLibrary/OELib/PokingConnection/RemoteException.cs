using System;

namespace OELib.PokingConnection
{
    [Serializable]
    public class RemoteException : Exception
    {
        private Exception Exception { get; }

        public RemoteException(Exception ex)
        {
            Exception = ex;
        }
    }
}