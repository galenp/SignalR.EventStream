using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using SignalR.Hubs;

namespace SignalR
{
    public class EventStream : Hub, IEventStream
    {
        #region " Send "
        public void Send(string @event)
        {
            Send("event", @event);
        }

        public void Send(string type, object @event)
        {
            SendToTarget("authorized", type, @event);
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
        public void SendToTarget(string target, string type, object @event)
        {
            GetClients<EventStream>()[target]
                .receiveEvent(JsonConvert.SerializeObject(
                    new {
                        Type = type,
                        Event = @event
                    }));
        }

        public void SendToTarget(string target, string @event)
        {
            SendToTarget(target, "event", @event);
        }

        public void SendToTarget(string target, object @event)
        {
            if (Utilities.IsAnonymousType(@event.GetType())) {
                throw new InvalidOperationException(
                    "Anonymous types are not supported. Use Send(string, object) instead.");
            }

            string type = @event.GetType().Name;
            SendToTarget(target, type, @event);
        }
        #endregion
        #region " SendToSelf "
        public void SendToSelf(string @event)
        {
            SendToSelf("event", @event);
        }

        public void SendToSelf(string type, object @event)
        {
            Caller
            .receiveEvent(JsonConvert.SerializeObject(
                new {
                    Type = type,
                    Event = @event
                }));
        }

        public void SendToSelf(object @event)
        {
            if (Utilities.IsAnonymousType(@event.GetType())) {
                throw new InvalidOperationException(
                    "Anonymous types are not supported. Use Send(string, object) instead.");
            }

            string type = @event.GetType().Name;
            SendToSelf(type, @event);
        }
        #endregion

        public void RaiseEvent(string data)
        {
            Send(data);
        }

        public bool Authorize(string authorizeFor = "authorized")
        {

            //use dependency injection to find if user is authorized
            var authorize = Infrastructure.DependencyResolver.Resolve<IStreamAuthorize>();
            if (authorize == null) {
                return false;
            }

            if (authorize.Authorized((string)Caller.ClientId, Context.User, authorizeFor)) {

                //validate user id
                //string id = Context.ClientId == "null" ? null : Context.ClientId;
                AddToGroup(authorizeFor);
                return true;

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

    }
}
