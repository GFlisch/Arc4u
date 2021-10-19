namespace Arc4u.KubeMQ
{
    public interface IMessageHandler<T>
    {
        void Handle(T message);
    }
}
