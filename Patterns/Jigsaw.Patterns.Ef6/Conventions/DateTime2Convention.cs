using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace Jigsaw.Infrastructure.Ef6.Conventions
{
    public class DateTime2Convention : Convention
    {
        public DateTime2Convention()
        {
            string[] specialColumns = new[] { "CreatedDate" };

            this.Properties<DateTime>()
                .Where(p => !specialColumns.Contains(p.Name))
                .Configure(c => c.HasColumnType("datetime2"));

            this.Properties<DateTime>()
                .Where(p => specialColumns.Contains(p.Name))
                .Configure(c => c.HasColumnType("datetime2")
                    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed)
                    );

        }
    }
}
