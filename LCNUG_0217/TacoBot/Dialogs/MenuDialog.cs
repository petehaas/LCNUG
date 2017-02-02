using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Bot.Builder.Location;
using System.Configuration;
using TacoBot.Assets;
using TacoBot.Models;
using TacoBot.Services;
using System.Globalization;
using TacoBot.Properties;

namespace TacoBot
{
    [Serializable]
    public class MenuDialog : PagedCarouselDialog<MenuItem>
    {
      
        private readonly IRepository<MenuItem> repository;

        public MenuDialog(IRepository<MenuItem> respository)
        {
            this.repository = respository;
        }

        public override string Prompt
        {
            get { return string.Format(CultureInfo.CurrentCulture, Resources.MenuDialog_Prompt); }
        }

        public override PagedCarouselCards GetCarouselCards(int pageNumber, int pageSize)
        {
            var pagedResult = this.repository.RetrievePage(pageNumber, pageSize, (menuitem) => menuitem.ItemName != "");

            var carouselCards = pagedResult.Items.Select(it => new HeroCard
            {
                Title = it.ItemName,
                Subtitle = it.ItemPrice.ToString("C") +  " " + it.ItemDescription,
                Images = new List<CardImage> { new CardImage(it.ItemPicture, it.ItemName) },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, Resources.MenuDialog_Select, value: it.ItemNumber.ToString()) }
            });

            return new PagedCarouselCards
            {
                Cards = carouselCards,
                TotalCount = pagedResult.TotalCount
            };
        }

        public override async Task ProcessMessageReceived(IDialogContext context, string itemNumber)
        {
            var tacoItem = this.repository.GetByID(Convert.ToInt32(itemNumber));
     
            if (tacoItem != null)
            {
                context.Done(tacoItem);
            }
            else
            {
                await context.PostAsync(string.Format(CultureInfo.CurrentCulture, Resources.MenuDialog_InvalidOption, itemNumber));
                await this.ShowProducts(context);
                context.Wait(this.MessageReceivedAsync);
            }
        }
        
    }
 
   
}