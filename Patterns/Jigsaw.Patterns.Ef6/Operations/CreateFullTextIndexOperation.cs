using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;

namespace Jigsaw.Infrastructure.Ef6.Operations
{
    public class CreateFullTextIndexOperation : MigrationOperation
    {
        public override bool IsDestructiveChange
        {
            get { return false; }
        }

        public string Table { get; set; }
        public string Index { get; set; }
        public IEnumerable<string> Columns { get; set; }

        public CreateFullTextIndexOperation(object anonymousArguments = null)
            : base(anonymousArguments)
        {
        }
    }
}
