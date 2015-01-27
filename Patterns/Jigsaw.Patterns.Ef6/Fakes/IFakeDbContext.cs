using System.Data.Entity;

namespace Jigsaw.Infrastructure.Ef6.Fakes
{
    public interface IFakeDbContext : IDataContextAsync
    {
        DbSet<T> Set<T>() where T : class;

        void AddFakeDbSet<TEntity, TFakeDbSet>()
            where TEntity : class , IObjectState, new()
            where TFakeDbSet : IDbSet<TEntity>, new();
    }
}
