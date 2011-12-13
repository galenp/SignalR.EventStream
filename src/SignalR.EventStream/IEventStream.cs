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
    }
}