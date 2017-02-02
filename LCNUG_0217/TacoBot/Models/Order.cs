using System;
using System.Collections.Generic;

namespace TacoBot.Models
{

    [Serializable]
    public class Order
   {
        public string OrderID { get; set; }
        public string RecipientName { get; set; }
        public string DeliveryAddress { get; set; }
        public List<TacoOrder> Items { get; set; }
        public DateTime DeliveryDate { get; set; }
        public bool Payed { get; set; }
        public string PaymentConfirmationToken { get; set; }

        public Order()
        {
           Items =  new List<TacoOrder>(); 
        }
    }
    
}