namespace TacoBot.Dialogs
{
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Properties;
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using TacoBot.Extensions;
    using TacoBot.Models;
    using TacoBot.Services;
    using TacoBot.Assets;
    using Microsoft.Bot.Builder.FormFlow;
    using System.Collections.Generic;
   
    using System.Configuration;
    using Microsoft.Bot.Builder.Location;
    using System.Linq;

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private readonly string checkoutUriFormat;
        private readonly ITacoBotDialogFactory dialogFactory;
        private readonly IOrdersService ordersService;
        private Models.Order order;
        private ResumptionCookie resumptionCookie;
        private bool openClosed;
        
        public RootDialog(string checkoutUriFormat, ITacoBotDialogFactory dialogFactory, IOrdersService ordersService)
        {
            this.checkoutUriFormat = checkoutUriFormat;
            this.dialogFactory = dialogFactory;
            this.ordersService = ordersService;
            this.order = new Order();
        }

        private bool CheckOpenClosed()
        {
            var dt = DateTime.Now;
            var start = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + "11:00:00"); // 11AM
            var end = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + "21:00:00"); // 9PM

            if (dt >= start && dt < end)
                return true;
            else
                return false;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.OnOptionSelected);
          //  await this.MessageReceivedAsync(context);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (this.resumptionCookie == null)
                this.resumptionCookie = new ResumptionCookie(message);

            // await this.WelcomeMessageAsync(context);
            context.Wait(this.OnOptionSelected);
        }

        private async Task WelcomeMessageAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();
         
            var options = new List<KeyValuePair<string,string>>();
            options.Add(new KeyValuePair<string,string>(Resources.RootDialog_Welcome_SeeMenu, Resources.RootDialog_Welcome_SeeMenu));
            options.Add(new KeyValuePair<string, string>(Resources.RootDialog_Welcome_Hours, Resources.RootDialog_Welcome_Hours));
            options.Add(new KeyValuePair<string, string>(Resources.RootDialog_Welcome_Directions, Resources.RootDialog_Welcome_Directions));
           
            reply.AddHeroCard(
                Resources.RootDialog_Welcome_Title,
                Resources.RootDialog_Welcome_Subtitle,
                options,
                new[] { "http://www.redlandstacoshack.com/images/TACO_LOGO.png" });

            await context.PostAsync(reply);

            context.Wait(this.OnOptionSelected);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (this.resumptionCookie == null)
                this.resumptionCookie = new ResumptionCookie(message);

            // User
            if (message.Text == Resources.RootDialog_Welcome_SeeMenu)
            {
                var menuDialog = this.dialogFactory.Create<MenuDialog>();
                context.Call(menuDialog, this.AfterMenuItemSelected);
            }
            else if (message.Text == Resources.RootDialog_Welcome_Hours)
            {
                var r = context.MakeMessage();
                r.Text = CheckOpenClosed() ? "Currently Open! " : "Sorry, we're closed ";
                r.Text += "Were open everyday from 11AM-9PM";
                await context.PostAsync(r);
                await this.StartOverAsync(context, Resources.RootDialog_Welcome_Menu);
            }
            else if (message.Text == Resources.RootDialog_Welcome_Directions)
            {
             
                var apiKey = ConfigurationManager.AppSettings["BingLocationAPIKey"];
                var options = LocationOptions.UseNativeControl | LocationOptions.ReverseGeocode;

                var requiredFields = LocationRequiredFields.StreetAddress | LocationRequiredFields.Locality |
                                     LocationRequiredFields.Region | LocationRequiredFields.Country |
                                     LocationRequiredFields.PostalCode;

                var prompt = "Please tell me your location";
            
                var locationDialog = new LocationDialog(apiKey, message.ChannelId, prompt, options, requiredFields,null, true, ConfigurationManager.AppSettings["TacoAddress"]);
                
                context.Call(locationDialog, this.ResumeAfterLocationDialogAsync);
            }
            else
            {
                await this.StartOverAsync(context, Resources.RootDialog_Welcome_Error);
            }
        }

        private async Task ResumeAfterLocationDialogAsync(IDialogContext context, IAwaitable<Place> result)
        {
            var place = await result;
            var formattedAddress = "";

            if (place != null)
            {
                var address = place.GetPostalAddress();
                formattedAddress = string.Join(", ", new[]
               {
                        address.StreetAddress,
                        address.Locality,
                        address.Region,
                        address.PostalCode,
                        address.Country
                    }.Where(x => !string.IsNullOrEmpty(x)));
                
            }

            await this.StartOverAsync(context, formattedAddress);
        }

        private async Task AfterLocationSelectied(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var m = await result as string;
                await this.StartOverAsync(context, m);
            }
            catch (TooManyAttemptsException)
            {

            }
        }

        private async Task AfterMenuItemSelected(IDialogContext context, IAwaitable<MenuItem> result)
        {
            var menuItem = await result;
            var tacoOrder = new TacoOrder();
            try
            {
                tacoOrder.TacoSelection = (TacoType)Enum.Parse(typeof(TacoType), menuItem.ItemNumber.ToString());
                var orderForm = new FormDialog<TacoOrder>(tacoOrder, TacoOrder.BuildForm, FormOptions.PromptInStart);
                context.Call(orderForm, this.AfterOrderForm);
            }
            catch (TooManyAttemptsException)
            {
                await this.StartOverAsync(context, Resources.RootDialog_Welcome_Error);
            }
        }

        private async Task AfterOrderForm(IDialogContext context, IAwaitable<TacoOrder> result)
        {
            var message = context.MakeMessage();
            var tacoSelection = await result;
            this.order.Items.Add(tacoSelection);

            PromptDialog.Confirm(context, OrderMoreTacos, "Would you like to order anything else?");
          
        }

        private async Task OrderMoreTacos(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                 var menuDialog = this.dialogFactory.Create<MenuDialog>();
                 context.Call(menuDialog, this.AfterMenuItemSelected);
            }
            else
            {
                var v = new InMemoryOrdersService();
                var pendingorder = v.PlacePendingOrder(this.order);
                var o = v.RetrieveOrder(pendingorder);
                var message = context.MakeMessage();
                message.Text = $"your order confirmation # is {o.OrderID}";
                await context.PostAsync(message);
                await this.ShowCart(context);
            }
           
        }
        
        private async Task ShowCart(IDialogContext context)
        {

            // Create header card.
            var reply = context.MakeMessage();
            var card = new CardAction();
            reply.Attachments = new List<Attachment>();
            List<CardAction> cardButtons = new List<CardAction>();
            List<CardImage> cardImages = new List<CardImage>();
            var items = new List<ReceiptItem>();
            var menu = new Services.InMemoryMenuRepository();
            decimal orderTotal = 0;
            foreach (var orderItem in this.order.Items)
            {

                // Find Matching Menu Item
                var selection = (int)orderItem.TacoSelection;
                var matchingMenuItem = menu.GetByID(selection);
                orderTotal += (matchingMenuItem.ItemPrice * orderItem.Quantity);
                items.Add(new ReceiptItem()
            {
                Subtitle = matchingMenuItem.ItemDescription,
                Price = (matchingMenuItem.ItemPrice * orderItem.Quantity).ToString("C"),
                Quantity = orderItem.Quantity.ToString(),
                Text = matchingMenuItem.ItemDescription,
                Title = matchingMenuItem.ItemName,
                Image = new CardImage() { Url = matchingMenuItem.ItemPicture }
            });
          }
         
            var tax = orderTotal * .07M;

            ReceiptCard plCard = new ReceiptCard()
            {
                Items = items,
                Tax = tax.ToString("C"),
                Total = (orderTotal + tax).ToString("C"),
                Title = "Shopping Cart"
            };

            Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);
            await context.PostAsync(reply);
            await this.StartOverAsync(context, "Your order has been submitted!");
        }
        
        private async Task StartOverAsync(IDialogContext context, string text)
        {
            this.order = new Models.Order();
            var message = context.MakeMessage();
            message.Text = text;
            await this.StartOverAsync(context, message);
        }

        private async Task StartOverAsync(IDialogContext context, IMessageActivity message)
        {
            this.order = new Models.Order();
            await context.PostAsync(message);
            await this.WelcomeMessageAsync(context);
        }
    }
}