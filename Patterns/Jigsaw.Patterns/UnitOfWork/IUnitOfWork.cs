using System;
using System.Data;

namespace Jigsaw
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : IObjectState;

        int SaveChanges();
        
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);

        void RunInTransaction(Action query);

        bool Commit();
        void Rollback();

        void Dispose(bool disposing);
    }
}
