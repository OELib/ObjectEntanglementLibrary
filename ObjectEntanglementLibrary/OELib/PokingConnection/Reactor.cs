#define DEBUGOUTPUT

using OELib.LibraryBase;
using OELib.LibraryBase.Messages;
using OELib.PokingConnection.Messages;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OELib.PokingConnection
{
    public class Reactor
    {
        public event EventHandler<Connection> RemoteReactingInspectionComplete;

        private Connection _connection;
        private object _reactingObject;

        private ObjectInfo _localReactingObjectInfo = null;

        private ObjectInfo LocalReactingObjectInfo
        {
            get
            {
                if (_localReactingObjectInfo == null) _localReactingObjectInfo = new ObjectInfo(_reactingObject);
                return _localReactingObjectInfo;
            }
        }

        private ObjectInfo _remoteReactingObjectInfo = null;
        private Task<TraceableMessage> remoteInspectionTask = null;

        public ObjectInfo RemoteReactingObjectInfo
        {
            get
            {
                remoteInspectionTask.Wait();
                return (remoteInspectionTask.Result as RemoteObjectInspectionResult).ObjectInfo;
            }
        }

        public Reactor(Connection connection, object reactingObject)
        {
            //connection.Name += $" ({reactingObject.ToString()})";
            _connection = connection;
            _reactingObject = reactingObject;
            _connection.MessageRecieved += _connection_MessageRecieved;
        }

        public void Start()
        {
#if (DEBUGOUTPUT)
            Debug.WriteLine($"Reactor of {_connection.Name} starting");
#endif
            remoteInspectionTask = _connection.AskAsync(new InspectRemoteObject());

            remoteInspectionTask.ContinueWith(res =>
            {
                //todo: result is null if the other side does not respond. take care of that
                _remoteReactingObjectInfo = (res.Result as RemoteObjectInspectionResult).ObjectInfo;
                RemoteReactingInspectionComplete?.Invoke(this, _connection);
            });
        }

        private void _connection_MessageRecieved(object sender, Message mes)
        {
#if (DEBUGOUTPUT)
            Debug.WriteLine($"Reactor of {_connection.Name} got message  {mes.ToString()}");
#endif
            if (mes is InspectRemoteObject)
            {
                //try
                //{
#if (DEBUGOUTPUT)
                //Debug.WriteLine($"Connection {_connection.Name} got inspection request");
#endif
                var result = new RemoteObjectInspectionResult(mes as InspectRemoteObject, LocalReactingObjectInfo);
                _connection.SendMessage(result);
                //}
                //catch (Exception ex)
                //{
                //    Debug.WriteLine("EXECPTION IN INSPECTION " + ex.ToString());
                //}
            }
            if (mes is CallMethod)
            {
                Exception exc = null;
                var callMessage = mes as CallMethod;
                object response = null;
                try
                {
                    response = _reactingObject.GetType().InvokeMember(callMessage.MethodName, BindingFlags.InvokeMethod, null, _reactingObject, callMessage.Arguments);
                }
                catch (Exception ex)
                {
                    exc = ex;
                }
                finally
                {
                    _connection.SendMessage(new CallMethodResponse(callMessage, response, exc));
                }
            }
        }

        public object CallRemoteMethod(string methodName, params object[] arguments)
        {
            if (IsMethodSupported(methodName) == false) throw new NotSupportedException($"Remote method {methodName} not supported");
            CallMethodResponse response = _connection.Ask(new CallMethod(methodName, arguments)) as CallMethodResponse;
            if (response == null) throw new NullResponseException();
            if (response.Exception != null) throw new RemoteException(response.Exception);
            return response.Response;
        }

        public object CallRemoteMethod(int timeout, string methodName, params object[] arguments)
        {
            if (IsMethodSupported(methodName) == false) throw new NotSupportedException($"Remote method {methodName} not supported");
            CallMethodResponse response = _connection.Ask(new CallMethod(methodName, arguments), timeout) as CallMethodResponse;
            if (response == null) throw new NullResponseException();
            if (response.Exception != null) throw new RemoteException(response.Exception);
            return response.Response;
        }

        public bool IsMethodSupported(string methodName) => RemoteReactingObjectInfo != null ? RemoteReactingObjectInfo.Methods.Any(mi => mi.Name == methodName) : false;

        public void SetProperty(string propertyName, object value)
        {
            throw new NotImplementedException();
        }

        public object GetProperty(string propertyName)
        {
            throw new NotImplementedException();
        }
    }
}