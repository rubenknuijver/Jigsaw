namespace Jigsaw
{
    public interface IQueryHandler<in TQuery, out TResult>
        where TQuery : IQueryObject<TResult>
    {
        TResult Handle(TQuery query);
    }
}
