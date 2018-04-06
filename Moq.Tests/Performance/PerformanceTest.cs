using System;
using System.Diagnostics;
using System.Threading;
using Moq.Performance;
using Moq.Performance.PerformanceModels;
using Xunit;
using Xunit.Abstractions;

namespace Moq.Tests.Performance
{
	public class PerformanceTest
	{
		private ITestOutputHelper output;

		public PerformanceTest(ITestOutputHelper output)
		{
			this.output = output;
		}
		
		[Fact]
		public void PerformanceMatchesReality()
		{
			IPerformanceModel testModel = new ConstantPerformanceModel(1000);
			IPerformanceModel realModel = new ConstantPerformanceModel(1000);
			
			IPerformanceContext performanceContext = new PerformanceContext();
			
			var actualClassUnderTest = new ClassUnderTest();
			var testClassUnderTest = new ClassUnderTest();
			var mockDependency = new Mock<ITestDependency>(performanceContext, MockBehavior.Default);
			mockDependency.Setup(x => x.DependencyFunction(testModel)).With(testModel);

			var realTime = actualClassUnderTest.FunctionUnderTest(realModel, new TestDependency());
			performanceContext.Run(() => testClassUnderTest.FunctionUnderTest(testModel, mockDependency.Object));

			output.WriteLine(performanceContext.TimelineVisualisation());
			Assert.True(Math.Abs(realTime - performanceContext.TimeTaken)/realTime * 100 < 5, $"{realTime}, {performanceContext.TimeTaken}");
		}
	}

	internal class ClassUnderTest
	{
		public long FunctionUnderTest(IPerformanceModel model, ITestDependency dependency)
		{
			var timer = Stopwatch.StartNew();
			
			// Work in function
			Thread.Sleep(10);
			
			// Work in dependency
			dependency.DependencyFunction(model);
			dependency.DependencyFunction(model);
			
			return timer.ElapsedMilliseconds;
		}
	}

	public interface ITestDependency
	{
		int DependencyFunction(IPerformanceModel model);
	}

	internal class TestDependency : ITestDependency
	{
		public int DependencyFunction(IPerformanceModel model)
		{
			Thread.Sleep((int) model.DrawTime());
			return 0;
		}
	}
}