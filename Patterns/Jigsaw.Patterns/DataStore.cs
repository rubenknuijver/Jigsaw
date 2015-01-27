using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jigsaw
{
    public abstract class AsyncDataStore
    {
        private readonly IUnitOfWorkAsync _uow;

        protected IUnitOfWorkAsync UnitOfWork
        {
            get { return _uow; }
        }

        public bool AutoSaveChanges
        {
            get;
            set;
        }

        protected AsyncDataStore(IUnitOfWorkAsync uow)
        {
            _uow = uow;
            AutoSaveChanges = true;
        }


        /// <summary>
        /// If AutoSaveChanges is set this method will auto commit changes
        /// </summary>
        /// <returns></returns>
        protected virtual Task<int> SaveChanges()
        {
            return SaveChanges(CancellationToken.None);
        }

        /// <summary>
        /// If AutoSaveChanges is set this method will auto commit changes
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual Task<int> SaveChanges(CancellationToken cancellationToken)
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

                try
                {
                    return _uow.SaveChangesAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    source.SetException(e);
                }
                finally
                {
                    registration.Dispose();
                }
            }
            return source.Task;
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

        abstract public void Cancel();
    }
}
