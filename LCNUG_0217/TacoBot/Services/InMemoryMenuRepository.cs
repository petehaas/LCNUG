namespace TacoBot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using TacoBot.Models;

    public class InMemoryMenuRepository : InMemoryRepositoryBase<MenuItem>
    {
        private List<MenuItem> menuItems;

        public InMemoryMenuRepository()
        {
            this.menuItems = new List<MenuItem>();

            // default image.
            //"http://images.media-allrecipes.com/userphotos/250x250/317051.jpg"
            var pic = "http://www.fatandsassymama.com/wp-content/uploads/2015/08/back1.jpg";
            this.menuItems.Add(new MenuItem() { ItemNumber = 1, ItemName = "California", ItemDescription = "Cilantro lime over fresh tilapia covered in avacado and slaw", ItemPicture = pic, ItemPrice = 8.99M });
            this.menuItems.Add(new MenuItem() { ItemNumber = 2, ItemName = "TexMex", ItemDescription = "Pulled pork with a hint of BBQ", ItemPicture = pic, ItemPrice = 8.99M });
            this.menuItems.Add(new MenuItem() { ItemNumber = 3, ItemName = "Fire", ItemDescription = "HOT Stuff! Blazing hot steak covered in hot peppers", ItemPicture = pic, ItemPrice = 8.99M });
            this.menuItems.Add(new MenuItem() { ItemNumber = 4, ItemName = "BYO", ItemDescription = "Build your own taco!  Pick any ingredient", ItemPicture = pic, ItemPrice = 10.99M });
            this.menuItems.Add(new MenuItem() { ItemNumber = 5, ItemName = "Pete's Special", ItemDescription = "grilled chicken with 3 kinds of peppers (Habanero, Jalapeno, Green) covered in Monerey Jack cheese. Pete's favorite!  ", ItemPicture = pic, ItemPrice = 9.99M });
            this.menuItems.Add(new MenuItem() { ItemNumber = 6, ItemName = "Traditional", ItemDescription = "A Classic taco with cilantro, tomato, and manchego cheese.  Topped with a marinated flank steak", ItemPicture = pic, ItemPrice = 8.99M });
            this.menuItems.Add(new MenuItem() { ItemNumber = 7, ItemName = "El Pastor", ItemDescription = "The classic pork taco covered in citrus cole slaw for a unique combination", ItemPicture = pic, ItemPrice = 9.99M });
        }

        public override IEnumerable<MenuItem> GetAll()
        {
            return this.menuItems;
        }

        public override MenuItem GetByName(string name)
        {
            return this.menuItems.SingleOrDefault(x => x.ItemName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }
            public override MenuItem GetByID(int id)
        {
            return this.menuItems.SingleOrDefault(x => x.ItemNumber.Equals(id));
        }

        protected override IEnumerable<MenuItem> Find(Func<MenuItem, bool> predicate)
        {
            return predicate != default(Func<MenuItem, bool>) ? this.menuItems.Where(predicate) : this.menuItems;
        }
    }
}