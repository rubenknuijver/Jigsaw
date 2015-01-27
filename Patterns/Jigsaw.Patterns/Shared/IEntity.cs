using System;

namespace Jigsaw.Shared
{
    public interface IEntity<out TKey> : IObjectState
    {
        [System.ComponentModel.DataAnnotations.Key]
        TKey Id { get; }
    }

    public interface IEntity : IEntity<Guid>
    {
    }
}
