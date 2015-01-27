using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Jigsaw.Infrastructure.Ef6.Fakes
{
    public class FakeDbContext : IFakeDbContext
    {
        private readonly Dictionary<Type, object> _fakeDbSets = new Dictionary<Type, object>();

        public Guid InstanceId
        {
            get;
            private set;
        }

        public FakeDbContext()
        {
            InstanceId = Guid.NewGuid();
        }

        public int SaveChanges()
        {
            return default(int);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(default(int));
        }

        public Task<int> SaveChangesAsync()
        {
            return Task.FromResult(default(int));
        }

        public void Dispose()
        {
        }

        public void SyncObjectState(object entity)
        {
        }

        public DbSet<T> Set<T>() where T : class
        {
            return (DbSet<T>)_fakeDbSets[typeof(T)];
        }

        public void AddFakeDbSet<TEntity, TFakeDbSet>()
            where TEntity : class, IObjectState, new()
            where TFakeDbSet : IDbSet<TEntity>, new()
        {
            _fakeDbSets.Add(typeof(TEntity), new TFakeDbSet());
        }

        public static class JsonSeeder
        {
            public static void FromStream<T>(IDbSet<T> dbSet, Stream stream, params FakeFieldMapping<T>[] additionalMapping) where T : class, IObjectState
            {
                using (var reader = new StreamReader(stream)) {
                    var jsonString = reader.ReadToEnd();
                    T[] modelCollection = JsonConvert.DeserializeObject<T[]>(jsonString);
                    foreach (var entity in modelCollection) {
                        dbSet.Add(entity);
                        foreach (var mapping in additionalMapping) {
                            mapping.Execute(entity, typeof(T).GetProperty(mapping.FieldName));
                        }
                    }
                }
            }

            public static void FromResource<T>(IDbSet<T> dbSet, string embeddedResourceName, params FakeFieldMapping<T>[] additionalMapping) where T : class,IObjectState
            {
                Assembly assembly = Assembly.GetCallingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(embeddedResourceName)) {
                    FromStream(dbSet, stream, additionalMapping);
                }
            }
        }
    }
}
