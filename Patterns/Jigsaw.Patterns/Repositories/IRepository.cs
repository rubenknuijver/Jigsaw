using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
#if ODATA
using System.Web.Http.OData.Query;
#endif

namespace Jigsaw
{
    public interface IRepository<TEntity> where TEntity : IObjectState
    {
        TEntity Find(params object[] keyValues);

        IQueryable<TEntity> SelectQuery(string query, params object[] parameters);

        void Insert(TEntity entity);
        void InsertRange(IEnumerable<TEntity> entities);
        void InsertGraph(TEntity entity);
        void InsertGraphRange(IEnumerable<TEntity> entities);

        void Update(TEntity entity);

        void Delete(object id);
        void Delete(TEntity entity);

        int Count();
        int Count(Expression<Func<TEntity, bool>> predicate);

        IQueryFluent<TEntity> Query(IQueryObject<TEntity> queryObject);
        IQueryFluent<TEntity> Query(Expression<Func<TEntity, bool>> query);
        IQueryFluent<TEntity> Query();

        IQueryable<TEntity> Queryable();
        IRepository<T> GetRepository<T>() where T : IObjectState;
#if ODATA
        IQueryable Queryable(ODataQueryOptions<TEntity> oDataQueryOptions);
#endif
    }
}
