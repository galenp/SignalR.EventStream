namespace SignalR
{
    public interface IEventStream
    {
        void Send(string @event);
        void Send(string type, object @event);
        void Send(object @event);
    }
}