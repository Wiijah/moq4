using System;
using System.Threading;
using System.Threading.Tasks;
using Moq.Performance;
using Moq.Performance.PerformanceModels;
using Xunit;

namespace Moq.Tests.Performance
{
	public class Demo
	{
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