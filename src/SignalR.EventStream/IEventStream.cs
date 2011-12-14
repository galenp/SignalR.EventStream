namespace SignalR
{
    public interface IEventStream
    {
        void Send(string @event);
        void Send(string type, object @event);
        void Send(object @event);

        void SendTo(string target, string @event);
        void SendTo(string target, object @event);
        void SendTo(string target, string type, object @event);

        dynamic Caller { get; }
        dynamic Clients { get; }
    }
}