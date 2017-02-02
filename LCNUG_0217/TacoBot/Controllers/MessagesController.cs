using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System.Collections.Generic;
using TacoBot.Services;
using TacoBot.Models;
using TacoBot.Properties;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;
using TacoBot;
using TacoBot.Extensions;

namespace TacoBot.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {

        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
           
            if (activity.Type == ActivityTypes.Message)
            {
               
                // The Configured IISExpressSSLPort property in this project file
                const int ConfiguredHttpsPort = 44371;

                var link = Url.Link("CheckOut", new { controller = "CheckOut", action = "Index" });
                var uriBuilder = new UriBuilder(link)
                {
                    Scheme = Uri.UriSchemeHttps,
                    Port = ConfiguredHttpsPort
                };
                var checkOutRouteUri = uriBuilder.Uri.ToString();

                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                {
                    var dialog = scope.Resolve<IDialog<object>>(TypedParameter.From(checkOutRouteUri));
                    await Conversation.SendAsync(activity, () => dialog);
                }
            }
            else
            {
                await this.HandleSystemMessage(activity);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
      
        private async Task HandleSystemMessage(Activity message)
        {
            await Task.CompletedTask;
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {

                StateClient stateClient = message.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(message.ChannelId, message.From.Id);
                var greeting = userData.GetProperty<bool>("SentGreeting");

                // We already sent a greeting message.
              //  if (greeting)
                //    return;

                var context = new ConnectorClient(new Uri(message.ServiceUrl));

                var reply = message.CreateReply();
             
                var options = new List<KeyValuePair<string, string>>();
                options.Add(new KeyValuePair<string, string>(Resources.RootDialog_Welcome_SeeMenu, Resources.RootDialog_Welcome_SeeMenu));
                options.Add(new KeyValuePair<string, string>(Resources.RootDialog_Welcome_Hours, Resources.RootDialog_Welcome_Hours));
                options.Add(new KeyValuePair<string, string>(Resources.RootDialog_Welcome_Directions, Resources.RootDialog_Welcome_Directions));

                reply.AddHeroCard(
                    Resources.RootDialog_Welcome_Title,
                    Resources.RootDialog_Welcome_Subtitle,
                    options,
                    new[] { "http://www.redlandstacoshack.com/images/TACO_LOGO.png" });
                
                 context.Conversations.ReplyToActivity(reply);

                // Set flag to turn off duplicate greeting messages.
                userData.SetProperty<bool>("SentGreeting", true);
                await stateClient.BotState.SetUserDataAsync(message.ChannelId, message.From.Id, userData);

            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            
        }
       
    }
}