using System.Data.Entity.ModelConfiguration.Conventions;

namespace Jigsaw.Infrastructure.Ef6.Conventions
{
    public class TableNameConvention : Convention
    {
        public TableNameConvention()
        {
            this.Types()
                .Configure(c => c.ToTable("Tbl" + c.ClrType.Name));
        }
    }
}
