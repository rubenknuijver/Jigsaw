using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Jigsaw.Shared
{
    public abstract class Entity : IObjectState
    {
        [NotMapped]
        public ObjectState ObjectState { get; set; }
    }

    public abstract class Entity<TKey> : Entity, IEntity<TKey>
    {
        [JsonProperty]
        [Key]
        public TKey Id { get; set; }
    }

    public abstract class DatedEntity<TKey> : Entity<TKey>, IDatedEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [JsonProperty(".created")]
        public virtual DateTime CreatedDate { get; set; }

        [JsonProperty(".updated")]
        public virtual DateTime UpdatedDate { get; set; }
    }
}
