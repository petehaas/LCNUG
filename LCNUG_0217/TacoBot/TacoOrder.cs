using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Connector;
using System.Linq;
using TacoBot.Services;

namespace TacoBot
{
    public enum TacoType { California = 1, TexMex, Fire, BYO, PetesSpecial, Traditional, ElPastor}
    public enum TacoMeat
    {
        Chicken = 1, Steak, Pork, Fish
    };
    public enum ShellOptions { Corn = 1, Flower, Bowl };
    public enum CheeseOptions { MontereyCheddar = 1, Pepperjack, Manchego };
    public enum ToppingOptions
    {
        Avocado = 1, GreenBellPeppers, Jalapenos,
        Lettuce, Onion, Tomatoes, Cilantro, HabaneroPeppers, BBQSalsa, CitrusSlaw
    };
     
    [Serializable]
    public class TacoOrder
    {
        public TacoType TacoSelection;
        public TacoMeat? Meat;
        public ShellOptions Shell;
        public CheeseOptions? Cheese;
        public List<ToppingOptions> Toppings;
        public int Quantity;

        public static IForm<TacoOrder> BuildForm()
        {
            OnCompletionAsyncDelegate<TacoOrder> processOrder = async (context, state) =>
            {
                await SetCart(context, state);
            };
  
            return new FormBuilder<TacoOrder>()
                     .Field(nameof(TacoSelection))
                     .Confirm(async (state) =>
                                       {
                                           switch (state.TacoSelection)
                                           {

                                               case TacoType.California:
                                                   state.Toppings = new List<ToppingOptions>();
                                                   state.Toppings.Add(ToppingOptions.Avocado);
                                                   state.Toppings.Add(ToppingOptions.Cilantro);
                                                   state.Shell = ShellOptions.Flower;
                                                   state.Meat = TacoMeat.Fish;
                                                   state.Cheese = CheeseOptions.Manchego;
                                                   break;

                                               case TacoType.TexMex:
                                                   state.Toppings = new List<ToppingOptions>();
                                                   state.Toppings.Add(ToppingOptions.BBQSalsa);
                                                   state.Toppings.Add(ToppingOptions.Onion);
                                                   state.Toppings.Add(ToppingOptions.Lettuce);
                                                   state.Meat = TacoMeat.Pork;
                                                   state.Cheese = CheeseOptions.MontereyCheddar;
                                                   break;

                                               case TacoType.Fire:
                                                   state.Toppings = new List<ToppingOptions>();
                                                   state.Toppings.Add(ToppingOptions.HabaneroPeppers);
                                                   state.Toppings.Add(ToppingOptions.Jalapenos);
                                                   state.Toppings.Add(ToppingOptions.Onion);
                                                   state.Meat = TacoMeat.Steak;
                                                   state.Cheese = CheeseOptions.Pepperjack;
                                                   break;

                                               case TacoType.PetesSpecial:
                                                   state.Toppings = new List<ToppingOptions>();
                                                   state.Toppings.Add(ToppingOptions.HabaneroPeppers);
                                                   state.Toppings.Add(ToppingOptions.Jalapenos);
                                                   state.Toppings.Add(ToppingOptions.GreenBellPeppers);
                                                   state.Meat = TacoMeat.Chicken;
                                                   state.Shell = ShellOptions.Corn;
                                                   state.Cheese = CheeseOptions.MontereyCheddar;
                                                   break;

                                               case TacoType.Traditional:
                                                   state.Toppings = new List<ToppingOptions>();
                                                   state.Toppings.Add(ToppingOptions.Cilantro);
                                                   state.Toppings.Add(ToppingOptions.Tomatoes);
                                                   state.Meat = TacoMeat.Steak;
                                                   state.Cheese = CheeseOptions.Manchego;
                                                   break;

                                               case TacoType.ElPastor:
                                                   state.Toppings = new List<ToppingOptions>();
                                                   state.Toppings.Add(ToppingOptions.CitrusSlaw);
                                                   state.Toppings.Add(ToppingOptions.Cilantro);
                                                   state.Toppings.Add(ToppingOptions.Onion);
                                                   state.Meat = TacoMeat.Pork;
                                                   state.Cheese = CheeseOptions.Manchego;
                                                   break;

                                               default: state.Meat = null; state.Toppings = null; state.Cheese = null; break; 
                                           }

                                           // get rid of the compiler highligting.
                                           await Task.CompletedTask;
                                           return new PromptAttribute($"You ordered the {state.TacoSelection} taco correct?");
                                       })
                    .AddRemainingFields()    
                //    .OnCompletion(processOrder)
                    .Build();
        }

        public static async Task SetCart(IDialogContext context, TacoOrder order)
        {
            context.UserData.SetValue<bool>("InitialConversation", false);
            
            // Create header card.
            var reply = context.MakeMessage();

            var card = new CardAction();

            reply.Attachments = new List<Attachment>();

            List<CardAction> cardButtons = new List<CardAction>();
            List<CardImage> cardImages = new List<CardImage>();

            var items = new List<ReceiptItem>();

            // Find Matching Menu Item
            var selection = (int)order.TacoSelection;
            
            var menu =  new Services.InMemoryMenuRepository();
            var matchingMenuItem = menu.GetByID(selection);
            items.Add(new ReceiptItem()
            {
                Subtitle = matchingMenuItem.ItemDescription,
                Price =matchingMenuItem.ItemPrice.ToString(),
                Quantity = order.Quantity.ToString(),
                Text =matchingMenuItem.ItemDescription,
                Title = matchingMenuItem.ItemName,
                Image = new CardImage() { Url = matchingMenuItem.ItemPicture }
            });

            var orderTotal = (matchingMenuItem.ItemPrice * order.Quantity);
            var tax = (matchingMenuItem.ItemPrice * order.Quantity) * .07M;

            ReceiptCard plCard = new ReceiptCard()
            {
                Items = items,
                Tax = tax.ToString("C"),
                Total = orderTotal.ToString("C"),
                Title = "Shopping Cart"
            };

            Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);
            await context.PostAsync(reply);

            //context(order);

        }
        
    }
}