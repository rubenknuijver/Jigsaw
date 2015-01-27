namespace Jigsaw.Specification
{
    internal class NotSpecification<TEntity> : ISpecification<TEntity>
    {
        private ISpecification<TEntity> _wrapped;

        internal NotSpecification(ISpecification<TEntity> spec)
        {
            _wrapped = spec;
        }

        public bool IsSatisfiedBy(TEntity candidate)
        {
            return !_wrapped.IsSatisfiedBy(candidate);
        }
    }
}
