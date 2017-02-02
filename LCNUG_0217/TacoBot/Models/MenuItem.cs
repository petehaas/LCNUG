using System;

namespace TacoBot.Models
{

        [Serializable]
        public class MenuItem
        {
            public int ItemNumber { get; set; }
            public string ItemName { get; set; }
            public string ItemDescription { get; set; }
            public decimal ItemPrice { get; set; }
            public string ItemPicture { get; set; }
        }
    
}