using System.Data.Entity.ModelConfiguration.Conventions;

namespace Jigsaw.Infrastructure.Ef6.Conventions
{
    public class IdColumnConvention : Convention
    {
        public IdColumnConvention()
        {
            //this.Properties<Guid>()
            //    .Where(w => w.Name == "Id")
            //    .Configure(c => c.IsKey()
            //        .HasColumnName(c.ClrPropertyInfo.ReflectedType.Name + "Id")
            //        );

            this.Properties()
                .Where(w => w.Name == "Id")
                .Configure(c => c.IsKey()
                    .HasColumnName(c.ClrPropertyInfo.ReflectedType.Name + "Id")
                   );
        }
    }
}
