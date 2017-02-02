namespace TacoBot.Dialogs
{
    using System.Collections.Generic;
    using TacoBot.Assets;
 

    public interface ITacoBotDialogFactory : IDialogFactory
    {
        SavedAddressDialog CreateSavedAddressDialog(
            string prompt,
            string useSavedAddressPrompt,
            string saveAddressPrompt,
            IDictionary<string, string> savedAddresses,
            IEnumerable<string> saveOptionNames);
    }
}