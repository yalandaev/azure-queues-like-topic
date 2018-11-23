using System;
using Autofac;

namespace MessageBus
{
    public class Bus
    {
        private IBusTransport _transport;

        public static Bus Instance { get; private set; }

        public static void RegisterEventQueues(IBusTransport transport)
        {
            Bus bus = new Bus {_transport = transport};
            transport.Initialize();
            Instance = bus;
        }

        private Bus()
        {
        }

        public void Publish(IEvent @event)
        {
            Console.WriteLine($"Bus.Publish(): Publish {@event.GetType()} event");
            _transport.PublishEvent(@event);
        }

        public void RegisterEventHandler<TEvent, THandler>(IContainer container) where THandler : IEventHandler<TEvent> where TEvent : IEvent
        {
            _transport.Subscribe<TEvent, THandler>(container);
        }
    }
}