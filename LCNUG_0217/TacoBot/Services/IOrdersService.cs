namespace TacoBot.Services
{
    using Models;

    public interface IOrdersService
    {
        string PlacePendingOrder(Order order);

        Order RetrieveOrder(string orderId);

        void ConfirmOrder(string orderId);
    }
}
