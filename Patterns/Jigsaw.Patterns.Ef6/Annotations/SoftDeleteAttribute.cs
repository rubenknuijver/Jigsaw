using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace Jigsaw.Infrastructure.Ef6.Annotations
{
    public class SoftDeleteAttribute : Attribute
    {
        public string ColumnName { get; set; }

        public SoftDeleteAttribute(string column)
        {
            ColumnName = column;
        }
        public static string GetSoftDeleteColumnName(EdmType type)
        {
            MetadataProperty annotation = type.MetadataProperties.SingleOrDefault(p => p.Name.EndsWith("customannotation:SoftDeleteColumnName"));

            return annotation == null ? null : (string)annotation.Value;
        }
    }
}
