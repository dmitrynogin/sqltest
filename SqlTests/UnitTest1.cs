using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlTests
{
    [TestClass]
    public class UnitTest1
    {
        const string CS = @"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = Test; Integrated Security = True; Connect Timeout = 30;";
        const string Update = "UPDATE [People] SET [Name] = 'Tom' WHERE [Id] = 1";
        const string Select = "SELECT [Name] FROM [People] WHERE [Id] = 1";
        
        [TestMethod]
        public void Should_See_Uncommitted_Updates()
        {
            using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted
                }))
            {
                using (var c1 = new SqlConnection(CS))
                {
                    c1.Open();
                    var cmd = c1.CreateCommand();
                    cmd.CommandText = Update;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.ExecuteNonQuery();
                }

                using (var c2 = new SqlConnection(CS))
                {
                    c2.Open();
                    var cmd2 = c2.CreateCommand();
                    cmd2.CommandText = Select;
                    cmd2.CommandType = System.Data.CommandType.Text;
                    var name = cmd2.ExecuteScalar();

                    Assert.AreEqual("Tom", name);
                }
            }
        }

        [TestMethod]
        public void Should_See_Uncommitted_EF_Updates()
        {
            using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted
                }))
            {
                var ctx = new TestContext();
                var p = ctx.People.Find(1);
                p.Name = "Tom";
                ctx.SaveChanges();

                using (var c2 = new SqlConnection(CS))
                {
                    c2.Open();
                    var cmd2 = c2.CreateCommand();
                    cmd2.CommandText = Select;
                    cmd2.CommandType = System.Data.CommandType.Text;
                    var name = cmd2.ExecuteScalar();

                    Assert.AreEqual("Tom", name);
                }
            }
        }


        public class TestContext : DbContext
        {
            public TestContext()
                : base(CS)
            {
            }

            public DbSet<People> People { get; set; }
        }

        public class People
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
