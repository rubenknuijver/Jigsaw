using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jigsaw.Shared;

namespace Jigsaw.Infrastructure.Ef6
{
    public abstract class DataContext<TContext> : IDataContextAsync, IDisposable, IObjectContextAdapter, IDbContext where TContext : DbContext
    {
        private TContext _context;
        private readonly Guid _instanceId;

        public Guid InstanceId { get { return _instanceId; } }

        protected TContext Context
        {
            get
            {
                return _context;
            }
        }

        DbContext IDbContext.Context
        {
            get { return Context; }
        }

        ObjectContext IObjectContextAdapter.ObjectContext
        {
            get { return ((IObjectContextAdapter)Context).ObjectContext; }
        }

        protected DataContext(string nameOrConnectionString)
        {
            _instanceId = Guid.NewGuid();

            _context = CreateContext(nameOrConnectionString);
            Context.Configuration.AutoDetectChangesEnabled = false;
            Context.Configuration.LazyLoadingEnabled = false;
            Context.Configuration.ProxyCreationEnabled = false;
        }

        protected abstract TContext CreateContext(string nameOrConnectionString);


        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SyncObjectsStatePreCommit();
            var changesAsync = await Context.SaveChangesAsync(cancellationToken);
            SyncObjectsStatePostCommit();
            return changesAsync;
        }

        public async Task<int> SaveChangesAsync()
        {
            SyncObjectsStatePreCommit();
            var changesAsync = await Context.SaveChangesAsync();
            SyncObjectsStatePostCommit();
            return changesAsync;
        }

        public int SaveChanges()
        {
            SyncObjectsStatePreCommit();

            int changes = Context.SaveChanges();
            SyncObjectsStatePostCommit();
            return changes;
        }

        public void SyncObjectState(object entity)
        {
            Context.Entry(entity).State = ((IObjectState)entity).ObjectState.ConvertState();
        }

        protected DbSet<T> Set<T>() where T : class, IObjectState
        {
            return Context.Set<T>();
        }

        private void SyncObjectsStatePreCommit()
        {
            foreach (var dbEntityEntry in Context.ChangeTracker.Entries()) {
                dbEntityEntry.State = ((IObjectState)dbEntityEntry.Entity).ObjectState.ConvertState();
                if (dbEntityEntry.Entity as IDatedEntity != null)
                    ((IDatedEntity)dbEntityEntry.Entity).UpdatedDate = DateTime.UtcNow;
            }
        }

        private void SyncObjectsStatePostCommit()
        {
            foreach (var dbEntityEntry in Context.ChangeTracker.Entries()) {
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

        public void Dispose()
        {
            if (_context != null) {
                _context.Dispose();
                _context = null;
            }
        }
    }
}
