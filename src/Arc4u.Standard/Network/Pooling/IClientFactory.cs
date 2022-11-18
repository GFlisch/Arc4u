namespace Arc4u.Network.Pooling
{
    public interface IClientFactory<out T>
    {
        T CreateClient();
    }
}
