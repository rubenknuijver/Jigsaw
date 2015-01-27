using System;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using Jigsaw.Shared;

namespace Jigsaw.Infrastructure.Ef6
{
    public class DataContext : DbContext, IDataContextAsync
    {
        private readonly Guid _instanceId;

        public Guid InstanceId { get { return _instanceId; } }

        public DataContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            _instanceId = Guid.NewGuid();
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }


        public override int SaveChanges()
        {
            SyncObjectsStatePreCommit();

            int changes = base.SaveChanges();
            SyncObjectsStatePostCommit();
            return changes;
        }

        public override async Task<int> SaveChangesAsync()
        {
            SyncObjectsStatePreCommit();
            var changesAsync = await base.SaveChangesAsync();
            SyncObjectsStatePostCommit();
            return changesAsync;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SyncObjectsStatePreCommit();
            var changesAsync = await base.SaveChangesAsync(cancellationToken);
            SyncObjectsStatePostCommit();
            return changesAsync;
        }

        protected int InternalSaveChanges()
        {
            int changes = base.SaveChanges();
            SyncObjectsStatePostCommit();
            return changes;
        }

        protected async Task<int> InternalSaveChangesAsync(CancellationToken cancellationToken)
        {
            var changesAsync = await base.SaveChangesAsync(cancellationToken);
            SyncObjectsStatePostCommit();
            return changesAsync;
        }

        public void SyncObjectState(object entity)
        {
            Entry(entity).State = ((IObjectState)entity).ObjectState.ConvertState();
        }

        public new DbSet<T> Set<T>() where T : class, IObjectState
        {
            return base.Set<T>();
        }

        private void SyncObjectsStatePreCommit()
        {
            foreach (var dbEntityEntry in ChangeTracker.Entries()) {
                dbEntityEntry.State = ((IObjectState)dbEntityEntry.Entity).ObjectState.ConvertState();
                if (dbEntityEntry.Entity as IDatedEntity != null)
                    ((IDatedEntity)dbEntityEntry.Entity).UpdatedDate = DateTime.UtcNow;
            }
        }

        public void SyncObjectsStatePostCommit()
        {
            foreach (var dbEntityEntry in ChangeTracker.Entries()) {
                ((IObjectState)dbEntityEntry.Entity).ObjectState = dbEntityEntry.State.ConvertState();
            }
        }

        static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object)) {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

    }
}
