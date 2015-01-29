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
    public class DataStoreUnitOfWork : IUnitOfWorkAsync
    {
        private readonly IDataContextAsync _dataContext;
        private bool _disposed;
        private ObjectContext _objectContext;
        private Dictionary<string, object> _repositories;
        private DbTransaction _transaction;

        public bool AutoSaveChanges
        {
            get;
            set;
        }

        public DataStoreUnitOfWork(IDataContextAsync dataContext)
        {
            _dataContext = dataContext;
        }

        public Task<int> SaveChangesAsync()
        {
            return _dataContext.SaveChangesAsync();
        }

        public Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken)
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

        public IRepository<TEntity> Repository<TEntity>() where TEntity : IObjectState
        {
            return RepositoryAsync<TEntity>(); 
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

        /// <summary>
        /// If AutoSaveChanges is set this method will auto commit changes
        /// </summary>
        /// <returns></returns>
        protected virtual Task<int> AutoSaveChanges()
        {
            return AutoSaveChanges(CancellationToken.None);
        }

        /// <summary>
        /// If AutoSaveChanges is set this method will auto commit changes
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual Task<int> AutoSaveChanges(CancellationToken cancellationToken)
        {
            var source = new TaskCompletionSource<int>();
            if (AutoSaveChanges) {
                var registration = new CancellationTokenRegistration();
                if (cancellationToken.CanBeCanceled) {
                    if (cancellationToken.IsCancellationRequested) {
                        source.SetCanceled();
                        return source.Task;
                    }
                    registration = cancellationToken.Register(CancelIgnoreFailure);
                }

                try {
                    return SaveChangesAsync(cancellationToken);
                }
                catch (Exception e) {
                    source.SetException(e);
                }
                finally {
                    registration.Dispose();
                }
            }
            return source.Task;
        }

        public void BeginTransaction(System.Data.IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            _objectContext = ((IObjectContextAdapter)_dataContext).ObjectContext;
            if (_objectContext.Connection.State != ConnectionState.Open) {
                _objectContext.Connection.Open();
            }

            _transaction = _objectContext.Connection.BeginTransaction(isolationLevel);
        }

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
            //if (__createDataContext != null) {
            //    _dataContext.Dispose();
            //    _dataContext = __createDataContext();
            //}
        }

        public bool Commit()
        {
            _transaction.Commit();
            //if (__createDataContext != null) {
            //    _dataContext.Dispose();
            //    _dataContext = __createDataContext();
            //}
            return true;
        }

        public void Rollback()
        {
            _transaction.Rollback();
            ((DataContext)_dataContext).SyncObjectsStatePostCommit();
        }

        protected void CancelIgnoreFailure()
        {
            // This method is used to route CancellationTokens to the Cancel method.
            // Cancellation is a suggestion, and exceptions should be ignored
            // rather than allowed to be unhandled, as there is no way to route
            // them to the caller.  It would be expected that the error will be
            // observed anyway from the regular method.  An example is cancelling
            // an operation on a closed connection.
            try {
                Cancel();
            }
            catch (Exception) {
            }
        }

        public virtual void Cancel()
        {

        }


        public void Dispose(bool disposing)
        {
            if (!_disposed && disposing) {
                _dataContext.Dispose();
                _dataContext = null;
            }
            _disposed = true;
        }

        public void Dispose()
        {
            if (_objectContext != null && _objectContext.Connection.State == ConnectionState.Open) {
                _objectContext.Connection.Close();
            }

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DataStoreUnitOfWork()
        {
            Dispose(false);
        }
    }
}
