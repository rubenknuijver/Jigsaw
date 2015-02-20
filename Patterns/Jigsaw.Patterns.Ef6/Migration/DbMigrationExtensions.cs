using Jigsaw.Infrastructure.Ef6.Operations;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;

namespace Jigsaw.Infrastructure.Ef6
{
    public static class DbMigrationExtensions
    {
        public static void CreateFullTextIndex(
            this DbMigration migration,
            string table,
            string index,
            string[] columns)
        {
            var op = new CreateFullTextIndexOperation {
                Table = table,
                Index = index,
                Columns = columns
            };

            ((IDbMigration)migration).AddOperation(op);
        }

        public static void AuthorizeOnDatabase(this DbMigration migration, string databaseName, string username)
        {
            ((IDbMigration)migration).AddOperation(new DatabaseAuthorizeOperation(databaseName, username));
        }
    }

}
