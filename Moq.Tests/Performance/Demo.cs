using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Moq.Performance;
using Moq.Performance.PerformanceModels;
using MySql.Data.MySqlClient;
using Xunit;
using Xunit.Abstractions;

namespace Moq.Tests.Performance
{
	public class Demo
	{
		private ITestOutputHelper output;

		public Demo(ITestOutputHelper output)
		{
			this.output = output;
		}
		
		[Fact]
		public void TestMultithreading()
		{
			var perCont = new PerformanceContext();
			
			var cUT = new ClassUnderTest();
			var c1 = new Mock<IDependencyClass1>(perCont, MockBehavior.Default);
			var c2 = new Mock<IDependencyClass2>(perCont, MockBehavior.Default);
			
			c1.Setup(c => c.DoSomething()).With(new GaussianDistributionPerformanceModel(10000, 1000), w => true);
			c2.Setup(c => c.DoSomething()).With(new TimeSpan(0, 0, 0, 0, 20000), w => true);
			
			perCont.Run(() => cUT.DoSomething(c1.Object, c2.Object));
			
			Assert.True(perCont.TimeTaken.TotalMilliseconds > 29999 && perCont.TimeTaken.TotalMilliseconds < 30005, $"{perCont.TimeTaken.TotalMilliseconds}");
			
		}

		[Fact]
		public void TestQueryPerfNormal()
		{
			var query = new DbQuery();
			var timer = Stopwatch.StartNew();
			
			this.QueryFunction(query, 1000);
			timer.Stop();
			
			output.WriteLine(timer.Elapsed.ToString());
			Assert.True(timer.Elapsed < new TimeSpan(0, 2, 0));
		}

		[Fact]
		public void TestQueryPerfFramework()
		{
			var performanceContext = new PerformanceContext();
			var mockDbQuery = new Mock<IDbQuery>(performanceContext, MockBehavior.Default);
			
			mockDbQuery.Setup(q => q.Query(It.IsAny<MySqlConnection>())).With(new TimeSpan(0, 0, 0, 0, 650));
			performanceContext.Run(() =>
			{
				this.QueryFunction(mockDbQuery.Object, 1000);
			});
			
			output.WriteLine(performanceContext.TimeTaken.ToString());
			
			Assert.True(performanceContext.TimeTaken < new TimeSpan(0, 2, 0));
		}

		private void QueryFunction(IDbQuery query, int number)
		{
			string connectionString = "Server=localhost;Database=user_accounts;Uid=root;Pwd=root;";
			using (MySqlConnection conn = new MySqlConnection(connectionString))
			{
				conn.Open();
				for (int i = 0; i < number; i++) query.Query(conn);
				conn.Close();
			}
		}

		public interface IDbQuery
		{
			void Query(MySqlConnection conn);
		}

		private class DbQuery : IDbQuery
		{
			public void Query(MySqlConnection conn)
			{
				string queryString = "SELECT * FROM accounts ORDER BY name;";

				var sqlCommand = new MySqlCommand(queryString, conn);
				var reader = sqlCommand.ExecuteReader();

				while (reader.Read())
				{
			
				}
				reader.Close();
				
			}
		}
		
		private class ClassUnderTest
		{
			public void DoSomething(IDependencyClass1 c1, IDependencyClass2 c2)
			{
				c1.DoSomething();
				c2.DoSomething();

//			Parallel.For(0, 2, i =>
//				{
//					if (i == 0) c1.DoSomething();
//					else c2.DoSomething();
//				});
			}
		}
		
		public interface IDependencyClass1
		{
			void DoSomething();
		}
		
		public interface IDependencyClass2
		{
			void DoSomething();
		}

		private class DependencyClass1 : IDependencyClass1
		{
			public void DoSomething()
			{
				Thread.Sleep(100);
			}
		}
		
		private class DependencyClass2 : IDependencyClass2
		{
			public void DoSomething()
			{
				Thread.Sleep(200);
			}
		}
	}
}