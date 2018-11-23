using System;
using System.Collections.Concurrent;
using System.Threading;
using Autofac;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace MessageBus.Transport.AzureStorageQueue
{
    public class HandlerWatchdog<TEvent, THandler>
        where TEvent : IEvent
        where THandler: IEventHandler<TEvent>
    {
        private THandler _handler;
        private AzureMessageQueue<TEvent> _messageQueue;
        private ConcurrentDictionary<Type, object> _handlerQueuePool;

        public HandlerWatchdog(ConcurrentDictionary<Type, object> handlerQueuePool, IContainer container)
        {
            _handlerQueuePool = handlerQueuePool;
            _handler = container.Resolve<THandler>();

            var queueName = $"h-{typeof(THandler).Name}".ToLower();

            if (_handlerQueuePool.ContainsKey(typeof(THandler)))
            {
                _messageQueue = (AzureMessageQueue<TEvent>)_handlerQueuePool[typeof(TEvent)];
            }
            else
            {
                _messageQueue = new AzureMessageQueue<TEvent>(queueName);
                _handlerQueuePool.TryAdd(typeof(TEvent), _messageQueue);
            }
        }

        public void Run()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{typeof(THandler).Name} Watchdog started");
            Console.ResetColor();

            while (true)
            {
                //Look at queue
                var message = _messageQueue.Get();
                if (message != null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{typeof(THandler).Name} Watchdog: message recieved");
                    Console.ResetColor();
                    try
                    {
                        TEvent @event = message.Value.Value;
                        _handler.Handle(@event);
                        _messageQueue.Delete(message.Value.Key);
                    }
                    catch
                    {
                        //TODO: Throw to dlx
                    }
                }
                else
                {
                    Thread.Sleep(3000);
                }
            }
        }
    }
}