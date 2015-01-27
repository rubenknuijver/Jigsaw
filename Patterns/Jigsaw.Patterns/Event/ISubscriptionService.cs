using System.Collections.Generic;

namespace Jigsaw
{
    public interface ISubscriptionService
    {
        IEnumerable<ICommandHandler<T>> GetSubscriptions<T>();
    }
}
