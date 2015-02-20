using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Linq;

namespace Jigsaw.Infrastructure.Ef6.Operations
{
    public class DatabaseAuthorizeOperation : MigrationOperation
    {
        public string DatabaseName { get; set; }
        public string UserName { get; set; }

        public override bool IsDestructiveChange
        {
            get { return false; }
        }

        protected DatabaseAuthorizeOperation(object anonymousArguments)
            : base(anonymousArguments)
        {

        }

        public DatabaseAuthorizeOperation(string databaseName, string username)
            : base(null)
        {
            DatabaseName = databaseName;
            UserName = username;
        }
    }
}
