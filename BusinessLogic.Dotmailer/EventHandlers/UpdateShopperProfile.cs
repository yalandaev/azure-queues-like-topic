using MessageBus;
using MessageBus.Events;

namespace BusinessLogic.Dotmailer.EventHandlers
{
    public class UpdateShopperProfile : IEventHandler<OnShopperProfileChanged>
    {
        private readonly DotmailerService _dotmailerService;

        public UpdateShopperProfile(DotmailerService dotmailerService)
        {
            _dotmailerService = dotmailerService;
        }

        public void Handle(OnShopperProfileChanged @event)
        {
            _dotmailerService.UpdateShopperProfile(@event.ShopperId);
        }
    }
}