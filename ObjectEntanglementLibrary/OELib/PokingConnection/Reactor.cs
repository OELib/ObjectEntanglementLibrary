#define _DEBUGOUTPUT

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

        public int DefaultCallTimeout { get; set; } = 60000;
        public Priority DefaultCallPriority { get; set; } = Priority.Normal;

        public Reactor(Connection connection, object reactingObject)
        {
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

        private void _connection_MessageRecieved(object sender, Message message)
        {
#if (DEBUGOUTPUT)
            Debug.WriteLine($"Reactor of {_connection.Name} got message  {message.ToString()}");
#endif
            if (message is InspectRemoteObject)
            {
#if (DEBUGOUTPUT)
                Debug.WriteLine($"Connection {_connection.Name} got inspection request");
#endif
                var result = new RemoteObjectInspectionResult(message as InspectRemoteObject, LocalReactingObjectInfo);
                _connection.SendMessage(result);
            }
            if (message is CallMethod)
            {
                Exception exc = null;
                var callMessage = message as CallMethod;
                object response = null;
                try
                {
                    var method = _reactingObject.GetType()
                         .GetMethods()
                         .Where(m => ((!callMessage.IsGenericMethod && m.Name == callMessage.MethodName) ||  // Only match on name for non generic methods
                                      (callMessage.IsGenericMethod && m.Name == callMessage.MethodName &&  m.IsGenericMethod)) &&
                                     m.GetParameters().Length == callMessage.Arguments.Length) // Match the number of parameters between call and method
                         .FirstOrDefault();

                    if (method.ContainsGenericParameters)
                    {
                        var arguments = method.GetGenericArguments();
                        var genericDef = method.GetGenericMethodDefinition();
                        var methodGeneric = method.MakeGenericMethod(callMessage.GenericType);
                        response = methodGeneric.Invoke(_reactingObject, callMessage.Arguments);
                    }
                    else
                    {
                        response = method.Invoke(_reactingObject, callMessage.Arguments);
                    }
                }
                catch (Exception ex)
                {
                    exc = ex;
                }
                finally
                {
                    _connection.SendMessage(new CallMethodResponse(callMessage, response, exc) { Priority = message.Priority });
                }
            }
        }

        public void CallRemoteVoidMethod(Priority priority, string methodName, params object[] arguments)
        {
            if (IsMethodSupported(methodName) == false) throw new NotSupportedException($"Remote method {methodName} not supported");
            //TODO: check that the method is a void return method.
            _connection.SendMessage(new CallMethod(methodName, arguments, null) { Priority = priority });
        }

        public object CallRemoteMethod(Priority priority, int timeout, string methodName, params object[] arguments)
        {
            return SendCallToPeer(priority, timeout, methodName, arguments, null);
        }

        public object CallRemoteMethod<T>(Priority priority, int timeout, string methodName, params object[] arguments)
        {
            return SendCallToPeer(priority, timeout, methodName, arguments, typeof(T));
        }

        private object SendCallToPeer(Priority priority, int timeout, string methodName, object[] arguments, Type genericType)
        {
            if (IsMethodSupported(methodName, genericType != null) == false) throw new NotSupportedException($"Remote method {methodName} not supported");
            CallMethodResponse response = _connection.Ask(new CallMethod(methodName, arguments, genericType) { Priority = priority }, timeout) as CallMethodResponse;
            if (response == null) throw new NullResponseException();
            if (response.Exception != null) throw new RemoteException(response.Exception);
            return response.Response;
        }

        public object CallRemoteMethod(int timeout, string methodName, params object[] arguments)
            => CallRemoteMethod(DefaultCallPriority, timeout, methodName, arguments);

        public object CallRemoteMethod(string methodName, params object[] arguments)
            => CallRemoteMethod(DefaultCallPriority, DefaultCallTimeout, methodName, arguments);

        public bool IsMethodSupported(string methodName, bool IsGeneric = false) => RemoteReactingObjectInfo != null ? RemoteReactingObjectInfo.Methods.Any(
            mi => mi.Name == methodName && mi.IsGeneric == IsGeneric ) : false;

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