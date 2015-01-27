namespace Jigsaw
{
    public interface IEventPublisher
    {
        void Publish<T>(T eventMessage);
    }
}
