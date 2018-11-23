
using Autofac;
using BusinessLogic.Dotmailer.EventHandlers;
using MessageBus;
using MessageBus.Events;

namespace BusinessLogic.Dotmailer
{
    public static class BusExtensions
    {
        public static void RegisterDotmailerEventHandlers(this Bus bus, IContainer container)
        {
            Bus.Instance.RegisterEventHandler<OnShopperProfileChanged, UpdateShopperProfile>(container);
        }
    }
}