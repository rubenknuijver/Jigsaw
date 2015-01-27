using System.ComponentModel.DataAnnotations.Schema;

namespace Jigsaw
{
    public interface IObjectState
    {
        [NotMapped]
        ObjectState ObjectState { get; set; }
    }
}
