using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace MessageBus.Transport.AzureStorageQueue
{
    /// <summary>
    /// Create WD and copy messages to subscribed queues
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public class EventWatchdog<TEvent> where TEvent : IEvent
    {
        private AzureMessageQueue<TEvent> _messageQueue;
        private ConcurrentDictionary<Type, object> _eventQueuePool;
        private ConcurrentDictionary<Type, List<object>> _handlersQueuePool;

        public EventWatchdog(ConcurrentDictionary<Type, object> eventQueuePool, ConcurrentDictionary<Type, List<object>> handlersQueuePool)
        {
            _eventQueuePool = eventQueuePool;
            _handlersQueuePool = handlersQueuePool;

            var queueName = $"e-{typeof(TEvent).Name}".ToLower();

            if (_eventQueuePool.ContainsKey(typeof(TEvent)))
            {
                _messageQueue = (AzureMessageQueue<TEvent>)_eventQueuePool[typeof(TEvent)];
            }
            else
            {
                _messageQueue = new AzureMessageQueue<TEvent>(queueName);
                _eventQueuePool.TryAdd(typeof(TEvent), _messageQueue);
            }
        }

        public void Run()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"{typeof(TEvent).Name} Watchdog started");
            Console.ResetColor();

            while (true)
            {
                //Look at queue
                var message = _messageQueue.Get();
                if (message != null)
                {
                    try
                    {
                        TEvent @event = message.Value.Value;
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine($"{typeof(TEvent).Name} Watchdog: event recieved");
                        Console.WriteLine($"{typeof(TEvent).Name} Watchdog: send event {message.Value.Key} to binded queues");
                        Console.ResetColor();

                        Type eventType = @event.GetType();
                        if (_handlersQueuePool.ContainsKey(eventType))
                        {
                            var handlerQueues = _handlersQueuePool[eventType];
                            if(!handlerQueues.Any())
                                continue;

                            foreach (var handlerQueue in handlerQueues)
                            {
                                MethodInfo methodInfo = handlerQueue.GetType().GetMethod("Enqueue");
                                object[] parametersArray = new object[] { @event };
                                methodInfo.Invoke(handlerQueue, parametersArray);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Queue doesn't exists");
                        }

                        _messageQueue.Delete(message.Value.Key);
                    }
                    catch
                    {
                        //throw to dlx
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