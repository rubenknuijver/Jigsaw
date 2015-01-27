using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Jigsaw.Infrastructure.Ef6
{
    public class UnitOfWork : IUnitOfWorkAsync
    {
        private IDataContextAsync _dataContext;
        private bool _disposed;
        private ObjectContext _objectContext;
        private Dictionary<string, object> _repositories;
        private DbTransaction _transaction;
        private Func<IDataContextAsync> __createDataContext;

        public UnitOfWork(IDataContextAsync dataContext)
        {
            _dataContext = dataContext;
        }
        public UnitOfWork(Func<IDataContextAsync> createDataContext)
        {
            __createDataContext = createDataContext;
        }

        public int SaveChanges()
        {
            try {
                return _dataContext.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e) {
                foreach (var validationErrors in e.EntityValidationErrors) {
                    foreach (var validationError in validationErrors.ValidationErrors) {
                        // ToDo: Log this 4 real
                        System.Diagnostics.Debug.WriteLine("Class: {0}, Property: {1}, Error: {2}",
                            validationErrors.Entry.Entity.GetType().FullName,
                            validationError.PropertyName,
                            validationError.ErrorMessage);
                    }
                }
                throw;
            }
            catch (UpdateException e) {

                var sqlException = e.InnerException as System.Data.SqlClient.SqlException;
                if (sqlException != null) {
                    // ToDo: add OperationManager for managing retry 
                    if (!string.IsNullOrWhiteSpace(sqlException.Procedure))
                        throw new Exception(string.Format("Er is een fout opgetreden in procedure '{0}' melding is '{1}'", sqlException.Procedure, sqlException.Message), e);

                    throw new Exception(string.Format("Er is een fout opgetreden op '{0}' melding is '{1}'", sqlException.Server, sqlException.Message), e);
                }

                throw;
            }
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : IObjectState
        {
            return RepositoryAsync<TEntity>();
        }

        public Task<int> SaveChangesAsync()
        {
            return _dataContext.SaveChangesAsync();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _dataContext.SaveChangesAsync(cancellationToken);
        }

        public IRepositoryAsync<TEntity> RepositoryAsync<TEntity>() where TEntity : IObjectState
        {
            if (_repositories == null) {
                _repositories = new Dictionary<string, object>();
            }

            var type = typeof(TEntity).Name;

            if (_repositories.ContainsKey(type)) {
                return (IRepositoryAsync<TEntity>)_repositories[type];
            }

            var repositoryType = typeof(Repository<>);

            _repositories.Add(type, Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _dataContext, this));
            return (IRepositoryAsync<TEntity>)_repositories[type];
        }

        // Uncomment, if rather have IRepositoryAsync<TEntity> IoC vs. Reflection Activation
        //public IRepositoryAsync<TEntity> RepositoryAsync<TEntity>() where TEntity : IObjectState
        //{
        //    return ServiceLocator.Current.GetInstance<IRepositoryAsync<TEntity>>();
        //}

        #region Unit of Work Transactions
        //2014.04.01 Add IsolationLevel
        public void BeginTransaction(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.Unspecified)
        {
            _objectContext = ((IObjectContextAdapter)_dataContext).ObjectContext;
            if (_objectContext.Connection.State != ConnectionState.Open) {
                _objectContext.Connection.Open();
            }

            _transaction = _objectContext.Connection.BeginTransaction(isolationLevel);
        }

        public bool Commit()
        {
            _transaction.Commit();
            if (__createDataContext != null) {
                _dataContext.Dispose();
                _dataContext = __createDataContext();
            }
            return true;
        }

        public void Rollback()
        {
            _transaction.Rollback();
            ((DataContext)_dataContext).SyncObjectsStatePostCommit();
        }
        #endregion

        public void RunInTransaction(Action query)
        {
            using (var transaction = new TransactionScope()) {
                // For some reason, I get different behavior when I use this
                _objectContext = ((IObjectContextAdapter)_dataContext).ObjectContext;
                if (_objectContext.Connection.State != ConnectionState.Open) {
                    _objectContext.Connection.Open();
                }
                query();
                transaction.Complete();
            }

            // REVIEW: Make sure this is really needed. I kept running into exceptions when I didn't do this, but I may be doing it wrong.
            if (__createDataContext != null) {
                _dataContext.Dispose();
                _dataContext = __createDataContext();
            }
        }
        public void Dispose()
        {
            if (_objectContext != null && _objectContext.Connection.State == ConnectionState.Open) {
                _objectContext.Connection.Close();
            }

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing) {
                _dataContext.Dispose();
                _dataContext = null;
            }
            _disposed = true;
        }

        ~UnitOfWork()
        {
            Dispose(false);
        }
    }
}
