using System;

namespace Jigsaw.Shared
{
    public interface IDatedEntity
    {
        DateTime CreatedDate { get; set; }

        DateTime UpdatedDate { get; set; }
    }
}
