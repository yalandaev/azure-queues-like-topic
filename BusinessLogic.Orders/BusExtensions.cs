using BusinessLogic.Orders.EventHandlers;
using MessageBus;
using MessageBus.Events;
using Autofac;

namespace BusinessLogic.Orders
{
    public static class BusExtensions
    {
        public static void RegisterOrdersEventHandlers(this Bus bus, IContainer container)
        {
            Bus.Instance.RegisterEventHandler<OnShopperProfileChanged, UpdateShopperOrders>(container);
        }
    }
}