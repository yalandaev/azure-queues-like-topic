using System;

namespace BusinessLogic.Orders
{
    public class OrdersService
    {
        public void UpdateShopperOrders(long shopperId)
        {
            Console.WriteLine($"OrdersService: Updating shopper's orders; ShopperID: {shopperId}");
        }
    }
}