using System;
using System.Linq.Expressions;

namespace Jigsaw.Specification
{
    public class LambdaSpecification<TEntity> : ISpecification<TEntity>
    {
        private Expression<Func<TEntity, bool>> _expression;

        private readonly Lazy<Func<TEntity, bool>> _is;

        public Expression<Func<TEntity, bool>> Expression { get { return _expression; } }

        public LambdaSpecification(Expression<Func<TEntity, bool>> satisfiedBy)
        {
            _expression = satisfiedBy;
            _is = new Lazy<Func<TEntity, bool>>(() => satisfiedBy.Compile());
        }

        public bool IsSatisfiedBy(TEntity entity)
        {
            return _is.Value(entity);
        }
    }
}
