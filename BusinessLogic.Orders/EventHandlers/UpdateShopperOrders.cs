using MessageBus;
using MessageBus.Events;

namespace BusinessLogic.Orders.EventHandlers
{
    public class UpdateShopperOrders: IEventHandler<OnShopperProfileChanged>
    {
        private readonly OrdersService _ordersService;

        public UpdateShopperOrders(OrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        public void Handle(OnShopperProfileChanged @event)
        {
            _ordersService.UpdateShopperOrders(@event.ShopperId);
        }
    }
}