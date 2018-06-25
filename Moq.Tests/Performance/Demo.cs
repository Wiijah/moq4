using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
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

			Assert.True(perCont.TimeTaken.TotalMilliseconds > 29999 && perCont.TimeTaken.TotalMilliseconds < 30005,
				$"{perCont.TimeTaken.TotalMilliseconds}");

		}

		[Fact]
		public void TestQueryPerfNormal()
		{
			var query = new DbQuery();
			var timer = Stopwatch.StartNew();

			this.QueryFunction(query, 100);
			timer.Stop();

			output.WriteLine(timer.Elapsed.ToString());
			Assert.True(timer.Elapsed < new TimeSpan(0, 2, 0));
		}

		[Fact]
		public void TestQueryPerfFramework()
		{
			var qu = new DbQuery();
			IList<int> list;
			
			using (var conn = new MySqlConnection("Server=localhost;Database=employees;Uid=root;Pwd=root;")) {
				conn.Open();
				list = qu.Query(conn);
				conn.Close();
			}
			
			var performanceContext = new PerformanceContext();
			var mockDbQuery = new Mock<IDbQuery>(performanceContext, MockBehavior.Default);

			mockDbQuery.Setup(q => q.Query(It.IsAny<MySqlConnection>())).With(new TimeSpan(0, 0, 0, 0, 166)).Returns((List<int>) list);
			performanceContext.Run(() => { this.QueryFunction(mockDbQuery.Object, 100); });

			output.WriteLine(performanceContext.TimeTaken.ToString());

			Assert.True(performanceContext.TimeTaken < new TimeSpan(0, 2, 0));
		}

		private void QueryFunction(IDbQuery query, int number)
		{
			string connectionString = "Server=localhost;Database=employees;Uid=root;Pwd=root;";
			using (MySqlConnection conn = new MySqlConnection(connectionString))
			{
				conn.Open();
				for (int i = 0; i < number; i++)
				{
					var list = query.Query(conn);
				
					Quicksort(list.Select(it => it as IComparable).ToArray(), 0, list.Count - 1);
				}
				conn.Close();
			}
		}

		public interface IDbQuery
		{
			List<int> Query(MySqlConnection conn);
		}

		private class DbQuery : IDbQuery
		{
			public List<int> Query(MySqlConnection conn)
			{
				string queryString = "SELECT * FROM employees WHERE last_name LIKE '%Ri%' ORDER BY employees.emp_no DESC;";

				var sqlCommand = new MySqlCommand(queryString, conn);
				var reader = sqlCommand.ExecuteReader();

				var list = new List<int>();
				while (reader.Read())
				{
					list.Add(reader.GetInt32(0));
				}
				
				reader.Close();

				return list;
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

		private static void MergeSort(int[] input, int low, int high)
		{
			if (low < high)
			{
				int middle = (low / 2) + (high / 2);
				MergeSort(input, low, middle);
				MergeSort(input, middle + 1, high);
				Merge(input, low, middle, high);
			}
		}

		private static void MergeSort(int[] input)
		{
			MergeSort(input, 0, input.Length - 1);
		}

		private static void Merge(int[] input, int low, int middle, int high)
		{

			int left = low;
			int right = middle + 1;
			int[] tmp = new int[(high - low) + 1];
			int tmpIndex = 0;

			while ((left <= middle) && (right <= high))
			{
				if (input[left] < input[right])
				{
					tmp[tmpIndex] = input[left];
					left = left + 1;
				}
				else
				{
					tmp[tmpIndex] = input[right];
					right = right + 1;
				}

				tmpIndex = tmpIndex + 1;
			}

			if (left <= middle)
			{
				while (left <= middle)
				{
					tmp[tmpIndex] = input[left];
					left = left + 1;
					tmpIndex = tmpIndex + 1;
				}
			}

			if (right <= high)
			{
				while (right <= high)
				{
					tmp[tmpIndex] = input[right];
					right = right + 1;
					tmpIndex = tmpIndex + 1;
				}
			}

			for (int i = 0; i < tmp.Length; i++)
			{
				input[low + i] = tmp[i];
			}

		}

		public static string PrintArray(int[] input)
		{
			string result = String.Empty;

			for (int i = 0; i < input.Length; i++)
			{
				result = result + input[i] + " ";
			}

			if (input.Length == 0)
			{
				result = "Array is empty.";
				return result;
			}
			else
			{
				return result;
			}
		}
		
		private static void Quicksort(IComparable[] elements, int left, int right)
		{
			int i = left, j = right;
			IComparable pivot = elements[(left + right) / 2];
 
			while (i <= j)
			{
				while (elements[i].CompareTo(pivot) < 0)
				{
					i++;
				}
 
				while (elements[j].CompareTo(pivot) > 0)
				{
					j--;
				}
 
				if (i <= j)
				{
					// Swap
					IComparable tmp = elements[i];
					elements[i] = elements[j];
					elements[j] = tmp;
 
					i++;
					j--;
				}
			}
 
			// Recursive calls
			if (left < j)
			{
				Quicksort(elements, left, j);
			}
 
			if (i < right)
			{
				Quicksort(elements, i, right);
			}
		}
		
		private static void InsertionSort(int[] intArray)
		{
			for (int i = 0; i < intArray.Length; i++)

			{

				Console.WriteLine(intArray[i]);

			}

 

			int temp, j;

			for (int i = 1; i < intArray.Length; i++)

			{

				temp = intArray[i];

				j = i - 1;

 

				while (j >= 0 && intArray[j] > temp)

				{

					intArray[j + 1] = intArray[j];

					j--;

				}

 

				intArray[j + 1] = temp;

			}

			for (int i = 0; i < intArray.Length; i++)

			{

				Console.WriteLine(intArray[i]);

			}

		}
	}
}
