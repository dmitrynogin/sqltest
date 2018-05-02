using System;
using System.Data.SqlClient;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlTests
{
    [TestClass]
    public class UnitTest1
    {
        string CS { get; } = @"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = Test; Integrated Security = True; Connect Timeout = 30;";
        string Update { get; } = "UPDATE [People] SET [Name] = 'Tom' WHERE [Id] = 1";
        string Select { get; } = "SELECT [Name] FROM [People] WHERE [Id] = 1";


        [TestMethod]
        public void ShouldRead()
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
    }
}
