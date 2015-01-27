using System;
using System.Linq.Expressions;

namespace Jigsaw
{
    /// <summary>
    /// From the Spec Pattern
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IQueryObject<TEntity>
    {
        Expression<Func<TEntity, bool>> Query();
        Expression<Func<TEntity, bool>> And(Expression<Func<TEntity, bool>> query);
        Expression<Func<TEntity, bool>> Or(Expression<Func<TEntity, bool>> query);
        Expression<Func<TEntity, bool>> And(IQueryObject<TEntity> queryObject);
        Expression<Func<TEntity, bool>> Or(IQueryObject<TEntity> queryObject);
    }
}
