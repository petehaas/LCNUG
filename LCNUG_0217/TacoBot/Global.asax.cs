using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using AutoMapper;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Mvc;

namespace TacoBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            this.RegisterBotDependencies();

            Mapper.Initialize(cfg => cfg.CreateMap<Models.Order, TacoBot.Models.Order>());

            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        private void RegisterBotDependencies()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new ReflectionSurrogateModule());

            builder.RegisterModule<TacoBotModule>();

            builder.RegisterControllers(typeof(WebApiApplication).Assembly);

            builder.Update(Conversation.Container);

            DependencyResolver.SetResolver(new AutofacDependencyResolver(Conversation.Container));
        }
    }
}
