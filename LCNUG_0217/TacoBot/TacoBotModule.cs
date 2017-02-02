namespace TacoBot
{
    using Autofac;
    using TacoBot.Dialogs;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using TacoBot.Models;
    using System.Configuration;

    public class TacoBotModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<TacoBotDialogFactory>()
                .Keyed<ITacoBotDialogFactory>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterType<RootDialog>()
                .As<IDialog<object>>()
                .InstancePerDependency();

            builder.RegisterType<MenuDialog>()
            .InstancePerDependency();

          //  builder.RegisterType<Location.SimpleLocationDialog>()
          //      .InstancePerDependency();
            
            /*builder.RegisterType<TacoOrder>()
                .As<IDialog<object>>()
                .InstancePerLifetimeScope();
               */
            //   builder.RegisterType<SettingsScorable>()
            //       .As<IScorable<double>>()
            //      .InstancePerLifetimeScope();

            //  builder.RegisterType<FlowerCategoriesDialog>()
            //       .InstancePerDependency();

            //  builder.RegisterType<BouquetsDialog>()
            //      .InstancePerDependency();

            builder.RegisterType<AddressDialog>()
                .InstancePerDependency();

            builder.RegisterType<SavedAddressDialog>()
              .InstancePerDependency();

          //  builder.RegisterType<SettingsDialog>()
          //   .InstancePerDependency();

            // Service dependencies
            builder.RegisterType<Services.InMemoryOrdersService>()
                .Keyed<Services.IOrdersService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<Services.InMemoryMenuRepository>()
                .Keyed<Services.IRepository<MenuItem>>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();

            /*
            builder.RegisterType<Location.SimpleLocationDialog>()
             .WithParameter("bingMapsKey", ConfigurationManager.AppSettings["BingLocationAPIKey"])
             .Keyed<Services.ILocationService>(FiberModule.Key_DoNotSerialize)
             .AsImplementedInterfaces()
             .SingleInstance();
             */
        }
    }
}