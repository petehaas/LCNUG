namespace TacoBot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;

    public class InMemoryOrdersService : IOrdersService
    {
        private IList<Order> orders;

        public InMemoryOrdersService()
        {
            this.orders = new List<Order>();
        }

        public void ConfirmOrder(string orderId)
        {
            var order = this.RetrieveOrder(orderId);
            if (order == null)
            {
                throw new InvalidOperationException("Order ID not found.");
            }

            if (order.Payed)
            {
                throw new InvalidOperationException("Order already payed.");
            }

            order.Payed = true;
        }

        public string PlacePendingOrder(Order order)
        {
            order.OrderID = Guid.NewGuid().ToString();
            order.Payed = false;
            this.orders.Add(order);

            return order.OrderID;
        }

        public Order RetrieveOrder(string orderId)
        {
            return this.orders.FirstOrDefault(o => o.OrderID == orderId);
        }
    }
}