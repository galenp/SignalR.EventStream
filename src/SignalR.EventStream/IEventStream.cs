namespace SignalR
{
    public interface IEventStream
    {
        void Send(string @event);
        void Send(string type, object @event);
        void Send(object @event);

        void SendToSelf(string @event);
        void SendToSelf(string type, object @event);
        void SendToSelf(object @event);

        void SendToTarget(string target, string @event);
        void SendToTarget(string target, object @event);
        void SendToTarget(string target, string type, object @event);

        dynamic Caller { get; }
        dynamic Clients { get; }
    }
}