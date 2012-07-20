using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using SignalR.Hubs;

namespace SignalR
{
    internal class EventStreamConnectionManager
    {
        public ConcurrentDictionary<string, List<string>> Users { get; set; }
        private static object locker = new object();

        public EventStreamConnectionManager()
        {
            Users = new ConcurrentDictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddConnection(string clientId, string userId)
        {
            lock (locker) {
                if (!Users.ContainsKey(userId)) {
                    Users[userId] = new List<string>() { clientId };
                } else {
                    Users[userId].Add(clientId);
                }
            }
        }

        public IEnumerable<string> GetConnection(string userId)
        {
            if (Users.ContainsKey(userId))
                return Users[userId];

            return null;
        }

        public void RemoveUser(string clientId)
        {
            lock (locker) {
                foreach (var user in Users) {
                    if (user.Value.Contains(clientId)) {
                        user.Value.Remove(clientId);
                        if (user.Value.Count == 0) {
                            List<string> values;
                            Users.TryRemove(user.Key, out values);
                        }
                        break;
                    }
                }
            }
        }
    }

    public class EventStream : Hub, IEventStream, IDisconnect
    {
        private static readonly EventStreamConnectionManager ConnectionManager;
        static EventStream()
        {
            ConnectionManager = new EventStreamConnectionManager();
        }

        #region " Send "
        public void Send(string @event)
        {
            Send("event", @event);
        }

        public void Send(string type, object @event)
        {
            SendTo("authorized", type, @event);
        }

        public void Send(object @event)
        {
            if (Utilities.IsAnonymousType(@event.GetType())) {
                throw new InvalidOperationException(
                    "Anonymous types are not supported. Use Send(string, object) instead.");
            }

            string type = @event.GetType().Name;
            Send(type, @event);
        }
        #endregion
        #region " SendToTarget "
        public void SendTo(string target, string type, object @event)
        {
            //find the target
            var destinations = ConnectionManager.GetConnection(target);
            if (destinations != null) {
            } else {
                destinations = new[] {target};
            }

            var context = GlobalHost.ConnectionManager.GetHubContext<EventStream>();
            foreach (var destination in destinations)
            {
                context.Clients[destination].receiveEvent(JsonConvert.SerializeObject(
                        new {
                            Type = type,
                            Event = @event
                        }));
            }
        }

        public void SendTo(string target, string @event)
        {
            SendTo(target, "event", @event);
        }

        public void SendTo(string target, object @event)
        {
            if (Utilities.IsAnonymousType(@event.GetType())) {
                throw new InvalidOperationException(
                    "Anonymous types are not supported. Use Send(string, object) instead.");
            }

            string type = @event.GetType().Name;
            SendTo(target, type, @event);
        }
        #endregion

        public void RaiseEvent(string data)
        {
            Send(data);
        }

        public bool Authorize(string authorizeFor = "authorized")
        {

            //use dependency injection to find if user is authorized
            var authorize = GlobalHost.DependencyResolver.Resolve<IStreamAuthorize>();
            var context = GlobalHost.ConnectionManager.GetHubContext<EventStream>();
            if (authorize != null) {

                string userId = Context.ConnectionId;
                if (authorize.Authorized(ref userId, Context.User, authorizeFor)) {
                    //validate user id
                    //string id = Context.ClientId == "null" ? null : Context.ClientId;
                    if (userId == null) userId = Context.ConnectionId;

                    ConnectionManager.AddConnection(Context.ConnectionId, userId);
                    context.Groups.Add(userId, authorizeFor);
                    return true;
                }
            }
            return false;
        }

        private static class Utilities
        {
            public static bool IsAnonymousType(Type type)
            {
                if (type == null)
                    throw new ArgumentNullException("type");

                return (type.IsClass
                        && type.IsSealed
                        && type.BaseType == typeof(object)
                        && type.Name.StartsWith("<>", StringComparison.Ordinal)
                        && type.IsDefined(typeof(CompilerGeneratedAttribute), true));
            }
        }

        public Task Disconnect()
        {
            DisconnectClient(Context.ConnectionId);
            return null;
        }

        private void DisconnectClient(string clientId)
        {
            ConnectionManager.RemoveUser(clientId);
        }
    }
}
