using Autofac;

namespace MessageBus
{
    public interface IBusTransport
    {
        void Initialize();
        void PublishEvent(IEvent @event);
        void Subscribe<TEvent, THandler>(IContainer container) where THandler : IEventHandler<TEvent> where TEvent : IEvent;
    }
}