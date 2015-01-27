using System;

namespace Jigsaw
{
    public interface IDataContext : IDisposable
    {
        int SaveChanges();

        void SyncObjectState(object entity);
    }
}
