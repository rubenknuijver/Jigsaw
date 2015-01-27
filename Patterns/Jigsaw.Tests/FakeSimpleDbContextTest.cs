using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jigsaw;
using Jigsaw.Infrastructure.Ef6;
using Jigsaw.Infrastructure.Ef6.Fakes;
using System.Data.Entity;
using Jigsaw.Shared;

namespace Jigsaw.Tests
{
    [TestClass]
    public class FakeSimpleDbContextTest
    {
        private IDataContextAsync CreateDataContext()
        {
            var context = new FakeSimpleDbContext();
            FakeDbContext.JsonSeeder.FromResource(
                context.SimpleSet,
                "Jigsaw.Tests.JsonFiles.myclass.json",
                FakeFieldMapping<MyClass>.Create(p => p.CreatedDate, (@class, info) => @class.CreatedDate = DateTime.Now.AddMinutes(0 - new Random().Next(2, 3600))),
                FakeFieldMapping<MyClass>.Create(p => p.UpdatedDate, (@class, info) => @class.UpdatedDate = DateTime.Now)
                );

            return context;
        }

        [TestMethod]
        public void InitiatingRepository_Should_ResolveAllData()
        {
            IDataContextAsync context = CreateDataContext();
            IUnitOfWorkAsync uow = new UnitOfWork(context);

            var rep = uow.RepositoryAsync<MyClass>();

            Assert.AreEqual(4, rep.Count());
        }

        public class FakeSimpleDbContext : FakeDbContext
        {
            public IDbSet<MyClass> SimpleSet
            {
                get { return Set<MyClass>(); }
            }

            public FakeSimpleDbContext()
            {
                this.AddFakeDbSet<MyClass, FakeDbSet<MyClass>>();
            }
        }

        public class MyClass : DatedEntity<Guid>
        {
            public string Text { get; set; }

            public int Sequence { get; set; }

            public MyClass()
            {
                Id = Guid.NewGuid();
            }
        }
    }
}
