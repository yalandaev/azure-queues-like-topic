using System;
using Autofac;
using BusinessLogic.Dotmailer;
using BusinessLogic.Dotmailer.EventHandlers;
using BusinessLogic.Orders;
using BusinessLogic.Orders.EventHandlers;
using BusinessLogic.Shopper;
using MessageBus;
using MessageBus.Transport.AzureStorageQueue;

namespace StorageQueuesBus
{
    class Program
    {
        static void Main(string[] args)
        {
            //Set up DI
            var builder = new ContainerBuilder();
            builder.RegisterType<StorageQueueTransport>().As<IBusTransport>().SingleInstance();
            //Services
            builder.RegisterType<OrdersService>().SingleInstance();
            builder.RegisterType<DotmailerService>().SingleInstance();
            //EventHandlers
            builder.RegisterType<UpdateShopperOrders>().SingleInstance();
            builder.RegisterType<UpdateShopperProfile>().SingleInstance();
            IContainer container = builder.Build();

            Console.WriteLine("Press any key to raise event");
            Console.WriteLine();

            //Init event's queues and watchdogs
            Bus.RegisterEventQueues(container.Resolve<IBusTransport>());
            //Init event handler's queues and watchdogs
            Bus.Instance.RegisterOrdersEventHandlers(container);
            Bus.Instance.RegisterDotmailerEventHandlers(container);

            //Action
            Random rnd = new Random(100);
            ShopperService shopperService = new ShopperService();

            while (true)
            {
                
                Console.ReadKey();
                
                shopperService.UpdateProfile(rnd.Next(1, 150));
            }
        }
    }
}
