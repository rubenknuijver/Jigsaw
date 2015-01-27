namespace Jigsaw.Infrastructure.Ef6
{
    public class QueryHandler<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TResult : IObjectState
        where TQuery : IQueryObject<TResult>
    {
        private readonly IRepository<TResult> _repository;

        protected QueryHandler(IRepository<TResult> repository)
        {
            _repository = repository;
        }

        public virtual TResult Handle(TQuery query)
        {
            //return _repository.Query(query).;
            return default(TResult);
        }
    }

}
