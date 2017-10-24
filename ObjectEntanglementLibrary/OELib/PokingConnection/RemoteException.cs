using System;

namespace OELib.PokingConnection
{
    [Serializable]
    public class RemoteException : Exception
    {
        private Exception exception { get; }

        public RemoteException(Exception ex)
        {
            exception = ex;
        }
    }
}