using Jigsaw.Infrastructure.Ef6.Operations;
using System;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.SqlServer;
using System.Linq;

namespace Jigsaw.Infrastructure.Ef6
{
    public class CustomSqlServerMigrationSqlGenerator : SqlServerMigrationSqlGenerator
    {
        private readonly string[] _columns = { "CreatedDate", "UpdatedDate" };

        protected override void Generate(AddColumnOperation addColumnOperation)
        {
            SetDefaultValue(addColumnOperation.Column);
            base.Generate(addColumnOperation);
        }

        protected override void Generate(CreateTableOperation createTableOperation)
        {
            foreach (var columnModel in createTableOperation.Columns) {
                SetDefaultValue(columnModel);
            }

            base.Generate(createTableOperation);
        }

        protected override void Generate(MigrationOperation migrationOperation)
        {
            this.Generate((dynamic)migrationOperation);
            base.Generate(migrationOperation);
        }

        private void SetDefaultValue(System.Data.Entity.Migrations.Model.ColumnModel columnModel)
        {
            if (_columns.Contains(columnModel.Name)) {
                columnModel.DefaultValueSql = "GETUTCDATE()";
            }
            if (columnModel.ClrType == typeof(Guid) && columnModel.Name.EndsWith("Id")) {
                columnModel.DefaultValueSql = "NEWID()";
            }
        }

        public virtual void Generate(CreateFullTextIndexOperation createFullTextIndexOperation)
        {
            using (var writer = Writer()) {
                writer.WriteLine("IF(NOT EXISTS(SELECT * FROM SYS.fulltext_catalogs WHERE is_default = 1))");
                writer.WriteLine("BEGIN");
                writer.WriteLine("    CREATE FULLTEXT CATALOG DefaultFullTextCatalog AS DEFAULT");
                writer.WriteLine("END");

                writer.WriteLine();

                writer.WriteLine("CREATE FULLTEXT INDEX ON {0} ({1})", Name(createFullTextIndexOperation.Table), string.Join(", ", createFullTextIndexOperation.Columns.Select(c => Quote(c))));
                writer.WriteLine("KEY INDEX {0}", Quote(createFullTextIndexOperation.Index));
                writer.WriteLine("WITH CHANGE_TRACKING AUTO");

                Statement(writer.InnerWriter.ToString(), suppressTransaction: true);
            }
        }

        public virtual void Generate(DatabaseAuthorizeOperation databaseAuthorizeOperation)
        {
            using (var writer = Writer()) {
                writer.WriteLine("ALTER AUTHORIZATION ON DATABASE::{0} TO {1}",
                    Name(databaseAuthorizeOperation.DatabaseName),
                    databaseAuthorizeOperation.UserName
                    );

                this.Statement(writer);
            }
        }
    }
}
