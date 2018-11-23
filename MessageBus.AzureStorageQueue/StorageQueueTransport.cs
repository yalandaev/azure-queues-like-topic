using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;

// ReSharper disable once CheckNamespace
namespace MessageBus.Transport.AzureStorageQueue
{
    public class StorageQueueTransport: IBusTransport
    {
        private ConcurrentDictionary<Type, object> _eventQueuePool;
        private ConcurrentDictionary<Type, object> _handlerQueuePool;
        private ConcurrentDictionary<Type, List<object>> _handlersBindings;

        public StorageQueueTransport()
        {
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            int initialCapacity = 101;

            _eventQueuePool = new ConcurrentDictionary<Type, object>(concurrencyLevel, initialCapacity);
            _handlerQueuePool = new ConcurrentDictionary<Type, object>(concurrencyLevel, initialCapacity);
            _handlersBindings = new ConcurrentDictionary<Type, List<object>>(concurrencyLevel, initialCapacity);
        }

        public void Initialize()
        {
            //Make event queues
            var types = Assembly.Load("MessageBus.Events, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null").GetTypes();
            foreach (var type in types)
            {
                var d1 = typeof(EventWatchdog<>);
                Type[] typeArgs = {type};
                var makeme = d1.MakeGenericType(typeArgs);
                object eventWatchdog = Activator.CreateInstance(makeme, _eventQueuePool, _handlersBindings);

                Task.Run(() =>
                {
                    MethodInfo methodInfo = eventWatchdog.GetType().GetMethod("Run");
                    methodInfo.Invoke(eventWatchdog, null);
                });
            }
        }

        public void PublishEvent(IEvent @event)
        {
            Type eventType = @event.GetType();
            if (_eventQueuePool.ContainsKey(eventType))
            {
                var queue = _eventQueuePool[eventType];
                MethodInfo methodInfo = queue.GetType().GetMethod("Enqueue");
                object[] parametersArray = new object[] { @event };
                methodInfo.Invoke(queue, parametersArray);
            }
            else
            {
                Console.WriteLine($"Queue doesn't exists");
            }
        }

        public void Subscribe<TEvent, THandler>(IContainer container) where THandler : IEventHandler<TEvent> where TEvent : IEvent
        {
            //Создать watchdog
            HandlerWatchdog<TEvent, THandler> handlerWatchdog = new HandlerWatchdog<TEvent, THandler>(_handlerQueuePool, container);

            if (!_handlersBindings.ContainsKey(typeof(TEvent)))
                _handlersBindings.TryAdd(typeof(TEvent), new List<object>());
            var item = _handlersBindings[typeof(TEvent)];
            var queue = _handlerQueuePool[typeof(TEvent)];
            item.Add(queue);
            Task.Run(() => handlerWatchdog.Run());
        }
    }
}