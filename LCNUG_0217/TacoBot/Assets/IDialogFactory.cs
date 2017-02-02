namespace TacoBot.Assets
{
    public interface IDialogFactory
    {
        T Create<T>();

        T Create<T, U>(U parameter);
    }
}