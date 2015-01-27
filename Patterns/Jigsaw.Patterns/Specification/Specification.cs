using System;

namespace Jigsaw.Specification
{
    public class Specification<T> : ISpecification<T>
    {
        private Predicate<T> _expression;

        public Specification()
        {
        }

        public Specification(Predicate<T> satisfiedBy)
        {
            _expression = satisfiedBy;
        }
        public virtual bool IsSatisfiedBy(T entity)
        {
            return _expression(entity);
        }
    }

    public abstract class AbstractSpecification<T> : ISpecification<T>
    {
        public abstract bool IsSatisfiedBy(T entity);

        public ISpecification<T> And(ISpecification<T> specification)
        {
            return new AndSpecification<T>(this, specification);
        }

        public ISpecification<T> Or(ISpecification<T> specification)
        {
            return new OrSpecification<T>(this, specification);
        }

        public ISpecification<T> Not(ISpecification<T> specification)
        {
            return new NotSpecification<T>(specification);
        }
    }
}
