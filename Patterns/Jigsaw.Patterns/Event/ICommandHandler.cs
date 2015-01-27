namespace Jigsaw
{
    /// <summary>
    /// Consumer of an Event or Command
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface ICommandHandler<in TCommand>
    {
        void Handle(TCommand command);
    }
}
