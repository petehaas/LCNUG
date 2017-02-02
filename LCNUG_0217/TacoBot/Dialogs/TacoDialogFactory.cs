namespace TacoBot.Dialogs
{
    using Autofac;
    using System.Collections.Generic;
    using TacoBot.Assets;

    public class TacoBotDialogFactory : DialogFactory, ITacoBotDialogFactory
    {
        public TacoBotDialogFactory(IComponentContext scope)
            : base(scope)
        {
        }

        public SavedAddressDialog CreateSavedAddressDialog(
            string prompt,
            string useSavedAddressPrompt,
            string saveAddressPrompt,
            IDictionary<string, string> savedAddresses,
            IEnumerable<string> saveOptionNames)
        {
            return this.Scope.Resolve<SavedAddressDialog>(
                new NamedParameter("prompt", prompt),
                new NamedParameter("useSavedAddressPrompt", useSavedAddressPrompt),
                new NamedParameter("saveAddressPrompt", saveAddressPrompt),
                TypedParameter.From(savedAddresses),
                TypedParameter.From(saveOptionNames));
        }
    }
}