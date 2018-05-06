using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlTests
{
    [TestClass]
    public class UnitTest1
    {
        const string CS = @"Data Source = .; Initial Catalog = Test; Integrated Security = True; Connect Timeout = 30;";
        const string Select = "WAITFOR DELAY '00:00:05'; SELECT [Name] FROM [People] WHERE [Id] = 1;";

        [TestMethod]
        public void Should_Work()
        {
            using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted
                }))
            {
                var tx = Transaction.Current;
                var name = Task.Run(async () =>
                {
                    using (var scope2 = new TransactionScope(tx, TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var result = await Task.WhenAll(from i in Enumerable.Range(0, 2)
                                           select QueryAsync());

                        scope2.Complete();
                        return result[0];
                    }
                }).Result;

                Assert.AreEqual("Tom", name);
            }
        }

        async Task<string> QueryAsync()
        {
            using (var c2 = new SqlConnection(CS))
            {
                await c2.OpenAsync();
                var cmd2 = c2.CreateCommand();
                cmd2.CommandText = Select;
                cmd2.CommandType = System.Data.CommandType.Text;
                return await cmd2.ExecuteScalarAsync() as string;
            }
        }
    }
}



