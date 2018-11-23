using System;
using MessageBus;
using MessageBus.Events;

namespace BusinessLogic.Shopper
{
    public class ShopperService
    {
        public void UpdateProfile(long shopperId)
        {
            Console.WriteLine($"ShopperService.UpdateProfile()");
            Bus.Instance.Publish(new OnShopperProfileChanged { ShopperId = shopperId });
        }
    }
}