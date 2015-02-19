using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Jigsaw.Infrastructure.Ef6
{
    public interface IDbContext
    {
        DbContext Context { get; }
    }
}
