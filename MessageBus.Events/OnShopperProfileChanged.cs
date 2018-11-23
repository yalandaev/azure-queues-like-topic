namespace MessageBus.Events
{
    public class OnShopperProfileChanged: IEvent
    {
        public long ShopperId { get; set; }
    }
}